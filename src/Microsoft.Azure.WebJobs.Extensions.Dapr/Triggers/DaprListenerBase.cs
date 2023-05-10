// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Listeners;

    abstract class DaprListenerBase : IListener
    {
        readonly IDaprServiceListener serviceListener;

        public DaprListenerBase(IDaprServiceListener serviceListener)
        {
            this.serviceListener = serviceListener;
        }

        public abstract void AddRoute(IRouteBuilder routeBuilder);

        public virtual void Cancel()
        {
            // no-op by default
        }

        Task IListener.StartAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.EnsureStartedAsync(cancellationToken);
        }

        Task IListener.StopAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.DeregisterListenerAsync(this, cancellationToken);
        }

        internal abstract Task DispatchInternalAsync(HttpContext context);

        public async Task DispatchAsync(HttpContext context)
        {
            try
            {
                await this.DispatchInternalAsync(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // No-op. This is expected when the request is aborted.
            }
            catch (Exception ex)
            {
                if (ex is DaprException || ex is DaprSidecarNotPresentException)
                {
                    var exception = ex as DaprException;
                    context.Response.StatusCode = (int)exception!.StatusCode;
                    await context.Response.WriteAsync(exception!.Message);
                }
                else
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Function invocation failed: {ex.Message}");
                }
            }
        }

        public abstract void Dispose();
    }
}