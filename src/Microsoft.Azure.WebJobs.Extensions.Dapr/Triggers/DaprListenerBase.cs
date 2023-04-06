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
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Host.Listeners;

    abstract class DaprListenerBase : IListener
    {
        readonly DaprServiceListener serviceListener;

        public DaprListenerBase(DaprServiceListener serviceListener)
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
                // TODO: How do we handle failed function calls? We probably shouldn't 500, as they could retry indefinitely
                await this.DispatchInternalAsync(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // No-op. This is expected when the request is aborted.
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(string.Empty);
            }
        }

        public abstract void Dispose();
    }
}