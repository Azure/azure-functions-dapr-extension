// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DaprExtensionTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    sealed class DaprRuntimeEmulator : IDisposable
    {
        readonly ConcurrentQueue<SavedHttpRequest> requestBin = new ConcurrentQueue<SavedHttpRequest>();
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JToken?>> stateStore = 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, JToken?>>();

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
                    // https://github.com/dapr/docs/blob/master/reference/api/state_api.md
                    routes.MapPost("v1.0/state/{storeName}", this.OnSaveState);
                    routes.MapGet("v1.0/state/{storeName}/{key}", this.OnGetState);

                    // Service invocation APIs
                    // https://github.com/dapr/docs/blob/master/reference/api/service_invocation_api.md
                    routes.MapRoute("v1.0/invoke/{appId}/method/{methodName}", this.OnInvoke);

                    // PubSub APIs
                    // https://github.com/dapr/docs/blob/master/reference/api/pubsub_api.md
                    routes.MapPost("v1.0/publish/{topic}", this.OnPublish);

                    // Secrets API
                    // https://github.com/dapr/docs/blob/master/reference/api/secrets_api.md
                    routes.MapGet("v1.0/secrets/{storeName}/{name}", this.OnGetSecret);

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
            string storeName = (string)routeData.Values["storeName"];

            ConcurrentDictionary<string, JToken?> namedStore = this.stateStore.GetOrAdd(
                storeName,
                _ => new ConcurrentDictionary<string, JToken?>(StringComparer.OrdinalIgnoreCase));

            using var reader = new StreamReader(context.Request.Body);
            string jsonPayload = await reader.ReadToEndAsync();
            JArray entries = JArray.Parse(jsonPayload);
            foreach (JObject entry in entries)
            {
                string key = (string)entry["key"];
                JToken? value = entry["value"];

                if (value == null)
                {
                    namedStore.TryRemove(key, out JToken? _);
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
            string storeName = (string)routeData.Values["storeName"];
            string key = (string)routeData.Values["key"];

            ConcurrentDictionary<string, JToken?>? namedStore;
            if (!this.stateStore.TryGetValue(storeName, out namedStore))
            {
                context.Response.StatusCode = 404;
                return;
            }

            if (!namedStore.TryGetValue(key, out JToken? value) || value == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            // Add a dummy etag value
            context.Response.Headers.Append("ETag", "\"1\"");

            using var writer = new StreamWriter(context.Response.Body);
            await writer.WriteAsync(value.ToString(Formatting.None));
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
            // This is just one example. The actual set of key/value pairs may differ
            // depending on the secret store provider.
            string secretName = (string)context.GetRouteValue("name");
            await context.Response.WriteAsync(@$"{{""{secretName}"":""secret!""}}");
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
