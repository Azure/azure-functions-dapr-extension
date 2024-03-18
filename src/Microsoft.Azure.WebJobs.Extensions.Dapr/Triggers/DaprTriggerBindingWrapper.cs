// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Listeners;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    [SupportsRetry]
    internal class DaprTriggerBindingWrapper : ITriggerBinding
    {
        private readonly ITriggerBinding innerTriggerBinding;

        public DaprTriggerBindingWrapper(ITriggerBinding triggerBinding)
        {
            this.innerTriggerBinding = triggerBinding;
        }

        public Type TriggerValueType => this.innerTriggerBinding.TriggerValueType;

        public IReadOnlyDictionary<string, Type> BindingDataContract => this.innerTriggerBinding.BindingDataContract;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            return this.innerTriggerBinding.BindAsync(value, context);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return this.innerTriggerBinding.CreateListenerAsync(context);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return this.innerTriggerBinding.ToParameterDescriptor();
        }
    }
}