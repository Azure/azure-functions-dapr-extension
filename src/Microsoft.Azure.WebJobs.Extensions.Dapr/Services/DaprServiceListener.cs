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

    sealed class DaprServiceListener : IDisposable, IDaprServiceListener
    {
        readonly HashSet<DaprListenerBase> listeners = new HashSet<DaprListenerBase>();
        readonly HashSet<DaprTopicSubscription> topics = new HashSet<DaprTopicSubscription>(new DaprTopicSubscriptionComparer());
        readonly string appAddress;
        readonly ILogger logger;

        IWebHost? host;
        int serverStarted;

        public DaprServiceListener(ILoggerFactory loggerFactory, INameResolver resolver)
        {
            this.logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
            this.appAddress = GetDefaultAppAddress(resolver);
        }

        public void Dispose() => this.host?.Dispose();

        static string GetDefaultAppAddress(INameResolver resolver)
        {
            if (!int.TryParse(resolver.Resolve("DAPR_APP_PORT"), out int appPort))
            {
                appPort = 3001;
            }

            return $"http://127.0.0.1:{appPort}";
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