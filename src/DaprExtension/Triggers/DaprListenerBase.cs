// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
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

        public abstract void AddRoutes(TriggerRouteHandler triggerHandler);

        public abstract void DeleteRoutes(TriggerRouteHandler triggerHandler);

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

        public abstract Task DispatchAsync(HttpContext context);

        public abstract void Dispose();
    }
}
