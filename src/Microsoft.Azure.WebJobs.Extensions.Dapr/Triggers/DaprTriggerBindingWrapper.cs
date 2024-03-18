// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Listeners;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Microsoft.Azure.WebJobs.Host.Triggers;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    [SupportsRetry]
    internal class DaprTriggerBindingWrapper : ITriggerBinding
    {
        private readonly ITriggerBinding _innerTriggerBinding;

        public DaprTriggerBindingWrapper(ITriggerBinding triggerBinding)
        {
            _innerTriggerBinding = triggerBinding;
        }

        public Type TriggerValueType => _innerTriggerBinding.TriggerValueType;

        public IReadOnlyDictionary<string, Type> BindingDataContract => _innerTriggerBinding.BindingDataContract;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            return _innerTriggerBinding.BindAsync(value, context);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return _innerTriggerBinding.CreateListenerAsync(context);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return _innerTriggerBinding.ToParameterDescriptor();
        }
    }
}