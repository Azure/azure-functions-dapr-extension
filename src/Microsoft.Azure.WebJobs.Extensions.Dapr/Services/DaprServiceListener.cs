// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal sealed class DaprServiceListener : IDisposable, IDaprServiceListener
    {
        private const string MetadataApiUrl = "";
        readonly HashSet<DaprListenerBase> listeners = new HashSet<DaprListenerBase>();
        readonly HashSet<DaprTopicSubscription> topics = new HashSet<DaprTopicSubscription>(new DaprTopicSubscriptionComparer());
        readonly string appAddress;
        readonly string daprAddress;
        readonly bool shouldCheckSidecarMetadataOnHostStartup;
        readonly ILogger logger;
        private readonly IDaprClient daprClient;

        IWebHost? host;
        int serverStarted;

        public DaprServiceListener(ILoggerFactory loggerFactory, IDaprClient daprClient, INameResolver resolver)
        {
            this.logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
            this.daprClient = daprClient;

            this.appAddress = GetAppAddress(resolver);
            this.daprAddress = DaprServiceClient.GetDaprHttpAddress(this.logger, resolver);
            this.shouldCheckSidecarMetadataOnHostStartup = !IsSidecarMetadataCheckOnHostStartupDisabled(resolver);
        }

        public void Dispose() => this.host?.Dispose();

        static string GetAppAddress(INameResolver resolver)
        {
            if (!int.TryParse(resolver.Resolve(Constants.EnvironmentKeys.AppPort), out int appPort))
            {
                appPort = 3001;
            }

            return $"http://127.0.0.1:{appPort}";
        }

        internal static bool IsSidecarMetadataCheckOnHostStartupDisabled(INameResolver resolver)
        {
            return bool.TryParse(resolver.Resolve(Constants.EnvironmentKeys.DisableSidecarMetadataCheck), out bool disableSidecarCheck)
                ? disableSidecarCheck
                : false; // do not disable by default
        }

        public async Task EnsureStartedAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref this.serverStarted, 1, 0) == 0)
            {
                this.host = new WebHostBuilder()
                    .UseKestrel()
                    .ConfigureServices(s => s.AddRouting())
                    .UseUrls(this.appAddress)
                    .Configure(app =>
                    {
                        var routes = new RouteBuilder(app);
                        foreach (DaprListenerBase listener in this.listeners)
                        {
                            // CONSIDER: Each listener should return a route object (or a collection)
                            //           instead of having direct access to the builder. This will
                            //           improve encapsulation and enable better logging.
                            listener.AddRoute(routes);
                        }

                        // See https://docs.dapr.io/reference/api/pubsub_api/#provide-a-route-for-dapr-to-discover-topic-subscriptions
                        routes.MapGet("dapr/subscribe", this.GetTopicsAsync);

                        app.UseRouter(routes.Build());
                    })
                    .Build();

                this.logger.LogInformation($"Starting Dapr HTTP listener on {this.appAddress} with {this.listeners.Count} function listener(s) registered.");
                await this.host.StartAsync(cancellationToken);
                this.logger.LogInformation("Dapr HTTP host started successfully.");

                if (this.shouldCheckSidecarMetadataOnHostStartup)
                {
                    await this.WarnIfSidecarMisconfigured();
                }
            }
        }

        public async Task DeregisterListenerAsync(DaprListenerBase listener, CancellationToken cancellationToken)
        {
            this.listeners.Remove(listener);

            if (this.host != null &&
                this.listeners.Count == 0 &&
                Interlocked.CompareExchange(ref this.serverStarted, 0, 1) == 1)
            {
                this.logger.LogInformation($"Stopping Dapr HTTP listener.");
                await this.host.StopAsync(cancellationToken);
                this.logger.LogInformation($"Dapr HTTP host stopped successfully.");
            }
        }

        public void AddFunctionListener(DaprListenerBase daprListener)
        {
            if (this.serverStarted > 0)
            {
                throw new InvalidOperationException("Cannot add listeners after the host has been started.");
            }

            this.listeners.Add(daprListener);
        }

        public void RegisterTopic(DaprTopicSubscription topic)
        {
            if (this.topics.Add(topic))
            {
                this.logger.LogInformation("Registered topic: {PubSubName}/{TopicName} -> {Route}", topic.PubSubName, topic.Topic, topic.Route);
            }
        }

        Task GetTopicsAsync(HttpContext context)
        {
            string topicListJson = JsonSerializer.Serialize(this.topics, JsonUtils.DefaultSerializerOptions);
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(topicListJson);
        }

        /// <summary>
        /// Warns if any sidecar settings are misconfigured.
        /// </summary>
        internal async Task WarnIfSidecarMisconfigured()
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

            string resBody;

            try
            {
                var res = await this.daprClient.GetAsync(this.logger, $"{this.daprAddress}/v1.0/metadata", cancellationToken);
                resBody = await res.Content.ReadAsStringAsync();
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Failed to query the Metadata API, received status code {res.StatusCode}, response body: {resBody}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to query the Metadata API, exception: {ex}");
                return;
            }

            JsonDocument jsonDocument;

            try
            {
                jsonDocument = JsonDocument.Parse(resBody);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to deserialize the Metadata API response, exception: {ex}");
                return;
            }

            JsonElement root = jsonDocument.RootElement;
            if (root.TryGetProperty("appConnectionProperties", out JsonElement appConnectionProperties))
            {
                // We set the appAddress, so it's safe to assume it's in the format we expect.
                // appAddress is in the format "http://127.0.0.1:3001"
                string[] appAddressParts = this.appAddress.Substring("http://".Length).Split(':');
                string appChannelAddress = appAddressParts[0];
                int appPort = int.Parse(appAddressParts[1]);

                try
                {
                    if (appConnectionProperties.TryGetProperty("port", out var port))
                    {
                        // port is an int in the Metadata API response.
                        var portInt = port.GetInt32();
                        if (portInt != appPort)
                        {
                            this.logger.LogWarning($"The Dapr sidecar is configured to listen on port {portInt}, but the app server is running on port {appPort}. This may cause unexpected behavior, see https://aka.ms/azfunc-dapr-app-config-error.");
                        }
                    }
                    else
                    {
                        // Daprd sidecar does not have port configured.
                        this.logger.LogWarning($"The Dapr sidecar is not configured to listen on a port, but the app server is running on port {appPort}. This may cause unexpected behavior, see https://aka.ms/azfunc-dapr-app-config-error.");
                    }

                    // channelAddress is always present in appConnectionProperties.
                    string address = appConnectionProperties.GetProperty("channelAddress").GetRawText().Trim('"');
                    if (address != appChannelAddress)
                    {
                        this.logger.LogWarning($"The Dapr sidecar is configured to listen on host {address}, but the app server is running on host {appChannelAddress}. This may cause unexpected behavior, see https://aka.ms/azfunc-dapr-app-config-error.");
                    }
                }
                catch (Exception e)
                {
                    this.logger.LogError($"Failed to parse appConnectionProperties in Metadata API response body, exception: {e}");
                    return;
                }
            }
            else
            {
                this.logger.LogDebug("appConnectionProperties not found in metadata API, skipping sidecar configuration check.");
                return;
            }
        }

        /// <summary>
        /// Helper class to define comparer of Dapr Topic Subscription.
        /// </summary>
        class DaprTopicSubscriptionComparer : IEqualityComparer<DaprTopicSubscription>
        {
            public bool Equals(DaprTopicSubscription topic1, DaprTopicSubscription topic2)
            {
                if (topic2 == null && topic1 == null)
                {
                    return true;
                }
                else if (topic1 == null || topic2 == null)
                {
                    return false;
                }
                else if (

                    // pub/sub name and topic name are case-sensitive in dapr
                    // routing is handled by ASP.NET and is case-insensitive
                    topic1.PubSubName.Equals(topic2.PubSubName, StringComparison.Ordinal)
                    && topic1.Topic.Equals(topic2.Topic, StringComparison.Ordinal)
                    && topic1.Route.Equals(topic2.Route, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(DaprTopicSubscription topic)
            {
                return (topic.PubSubName, topic.Topic, topic.Route.ToLowerInvariant()).GetHashCode();
            }
        }
    }
}