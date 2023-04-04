// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit.Sdk;

    sealed class DaprRuntimeEmulator : IDisposable
    {
        readonly ConcurrentQueue<SavedHttpRequest> requestBin = new ConcurrentQueue<SavedHttpRequest>();
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> stateStore =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, object?>>();

        readonly IWebHost host;

        public DaprRuntimeEmulator(int daprPort)
        {
            this.host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(s => s.AddRouting())
                .UseUrls($"http://localhost:{daprPort}")
                .Configure(app =>
                {
                    // Middleware to save requests into a request bin
                    app.Use(async (context, next) =>
                    {
                        await this.SaveRequestAsync(context.Request);
                        await next();
                    });

                    var routes = new RouteBuilder(app);

                    // State APIs
                    // https://docs.dapr.io/reference/api/state_api/
                    routes.MapPost("v1.0/state/{storeName}", this.OnSaveState);
                    routes.MapGet("v1.0/state/{storeName}", this.OnGetState);
                    routes.MapGet("v1.0/state/{storeName}/{key}", this.OnGetState);

                    // Service invocation APIs
                    // https://docs.dapr.io/reference/api/service_invocation_api/
                    routes.MapRoute("v1.0/invoke/{appId}/method/{methodName}", this.OnInvoke);

                    // PubSub APIs
                    // https://docs.dapr.io/reference/api/pubsub_api/
                    routes.MapPost("v1.0/publish/{name}/{topic}", this.OnPublish);

                    // Secrets API
                    // https://docs.dapr.io/reference/api/secrets_api/
                    routes.MapGet("v1.0/secrets/{storeName}/{name}", this.OnGetSecret);

                    // Output binding API
                    // https://docs.dapr.io/reference/api/bindings_api/
                    routes.MapPost("v1.0/bindings/{name}", this.OnSendMessage);

                    app.UseRouter(routes.Build());
                })
                .Build();
        }

        public void Dispose() => this.host.Dispose();

        public Task StartAsync() => this.host.StartAsync();

        public Task StopAsync() => this.host.StopAsync();

        async Task OnSaveState(HttpContext context)
        {
            RouteData routeData = context.GetRouteData();
            string storeName = Uri.UnescapeDataString((string)routeData.Values["storeName"]);

            ConcurrentDictionary<string, object?> namedStore = this.stateStore.GetOrAdd(
                storeName,
                _ => new ConcurrentDictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

            using var reader = new StreamReader(context.Request.Body);
            string jsonPayload = await reader.ReadToEndAsync();
            JsonDocument entries = JsonDocument.Parse(jsonPayload);
            foreach (JsonProperty prop in entries.RootElement.EnumerateObject())
            {
                string key = prop.Name;
                JsonElement? value = prop.Value;

                if (value == null)
                {
                    namedStore.TryRemove(key, out object? _);
                }
                else
                {
                    namedStore[key] = value;
                }
            }
        }

        async Task OnGetState(HttpContext context)
        {
            RouteData routeData = context.GetRouteData();
            string storeName = Uri.UnescapeDataString((string)routeData.Values["storeName"]);
            string key = (string)routeData.Values["key"];
            if (string.IsNullOrEmpty(key))
            {
                context.Response.StatusCode = 204;
                return;
            }

            ConcurrentDictionary<string, object?>? namedStore;
            if (!this.stateStore.TryGetValue(storeName, out namedStore))
            {
                context.Response.StatusCode = 204;
                return;
            }

            if (!namedStore.TryGetValue(key, out object? value) || value == null)
            {
                context.Response.StatusCode = 204;
                return;
            }

            // Add a dummy etag value
            context.Response.Headers.Append("ETag", "\"1\"");

            using var writer = new StreamWriter(context.Response.Body);
            await writer.WriteAsync(JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Directly retrieve the saved state from mock state store for unit testing
        /// </summary>
        /// <returns></returns>
        internal object? FetchSavedStateForUnitTesting(string stateStore, string key)
        {
            try
            {
                return this.stateStore[stateStore]?[key];
            }
            catch
            {
                throw new XunitException($"The state with key ({key}) was not found in state store ({stateStore}).");
            }
        }

        /// <summary>
        /// Directly write to the state store for unit testing.
        /// </summary>
        internal void SaveStateForUnitTesting(string storeName, string key, object value)
        {
            ConcurrentDictionary<string, object?> namedStore = this.stateStore.GetOrAdd(
                storeName,
                _ => new ConcurrentDictionary<string, object?>(StringComparer.OrdinalIgnoreCase));

            namedStore[key] = value;
        }

        async Task OnInvoke(HttpContext context)
        {
            await Task.CompletedTask; // TODO
        }

        async Task OnPublish(HttpContext context)
        {
            await Task.CompletedTask; // TODO
        }

        async Task OnGetSecret(HttpContext context)
        {
            // https://docs.dapr.io/reference/api/secrets_api/
            // This is an example that supports multiple kyes in one secret.
            // Depending on the secret store provider, the binding should return a dictionary of multiple or just one key-value pair
            string secretName = (string)context.GetRouteValue("name");
            await context.Response.WriteAsync(@$"{{""{secretName}1"":""secret!"", ""{secretName}2"":""another secret!""}}");
        }

        async Task OnSendMessage(HttpContext context)
        {
            await Task.CompletedTask;
        }

        async Task SaveRequestAsync(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            string content = await reader.ReadToEndAsync();

            // Add a copy of the request because the original object will be
            // uninitialized by the time someone tries to get it.
            this.requestBin.Enqueue(new SavedHttpRequest(request, content));

            request.Body.Position = 0;
        }

        internal SavedHttpRequest[] GetReceivedRequests() => this.requestBin.ToArray();
    }
}
