// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    sealed class DaprServiceListener : IDisposable
    {
        readonly ConcurrentDictionary<PathString, DaprListenerBase> listeners;
        readonly ILogger log;
        readonly IWebHost host;

        int serverStarted = 0;

        public DaprServiceListener(ILoggerFactory loggerFactory)
        {
            this.listeners = new ConcurrentDictionary<PathString, DaprListenerBase>();
            this.log = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
            this.host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:3001") // TODO: Configurable
                .Configure(a => a.Run(this.HandleRequestAsync))
                .Build();
        }

        public void Dispose() => this.host.Dispose();

        internal async Task RegisterListenerAsync(DaprListenerBase listener, CancellationToken cancellationToken)
        {
            if (this.listeners.TryAdd(listener.ListenPath, listener))
            {
                // TODO: Probably need the method info here as well
                this.log.LogInformation("Registered function listener for path {Path}.", listener.ListenPath);
            }

            if (Interlocked.CompareExchange(ref this.serverStarted, 1, 0) == 0)
            {
                await this.host.StartAsync(cancellationToken);
            }
        }

        internal async Task DeregisterListenerAsync(DaprListenerBase listener, CancellationToken cancellationToken)
        {
            // TODO: Lock
            if (this.listeners.TryRemove(listener.ListenPath, out _))
            {
                // TODO: Probably need the method info here as well
                this.log.LogInformation("Deregistered function listener for path {Path}.", listener.ListenPath);
            }

            if (this.listeners.Count == 0 && Interlocked.CompareExchange(ref this.serverStarted, 0, 1) == 1)
            {
                await this.host.StopAsync(cancellationToken);
            }
        }

        async Task HandleRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            this.log.LogTrace("Received request: {Method} {Path}", request.Method, request.Path);
            try
            {
                await this.DispatchToListener(context);
            }
            catch (Exception unexpectedException)
            {
                // TODO: Proper tracing
                this.log.LogError(
                    unexpectedException,
                    "Unhandled exception in HTTP API handler for {Method} {Path}",
                    request.Method,
                    request.Path);

                context.Response.StatusCode = 500;
            }

            this.log.LogTrace(
                "Sending response: {Method} {Path} -> {StatusCode}. Content length: {ContentLength}",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                context.Response.ContentLength ?? -1);
        }

        async Task DispatchToListener(HttpContext context)
        {
            if (this.listeners.TryGetValue(context.Request.Path, out DaprListenerBase listener))
            {
                await listener.DispatchAsync(context);
            }
            else
            {
                this.log.LogWarning(
                    "No listener was registered that could handle {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                context.Response.StatusCode = 404;
            }
        }
    }
}
