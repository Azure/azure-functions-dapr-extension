// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    abstract class DaprListenerBase : IListener
    {
        readonly DaprServiceListener serviceListener;

        public DaprListenerBase(DaprServiceListener serviceListener, PathString listenPath)
        {
            this.serviceListener = serviceListener;
            this.ListenPath = listenPath;
        }

        public PathString ListenPath { get; }

        public virtual void Cancel()
        {
            // no-op by default
        }

        Task IListener.StartAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.RegisterListenerAsync(this, cancellationToken);
        }

        Task IListener.StopAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.DeregisterListenerAsync(this, cancellationToken);
        }

        public abstract Task DispatchAsync(HttpContext context);

        public abstract void Dispose();
    }
}
