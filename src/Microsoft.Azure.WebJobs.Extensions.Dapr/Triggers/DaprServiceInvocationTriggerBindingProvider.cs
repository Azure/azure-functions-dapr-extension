// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    class DaprServiceInvocationTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;
        readonly INameResolver nameResolver;

        public DaprServiceInvocationTriggerBindingProvider(DaprServiceListener serviceListener, INameResolver resolver)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
            this.nameResolver = resolver;
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprServiceInvocationTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return BindingUtils.NullTriggerBindingTask;
            }

            string methodName = TriggerHelper.ResolveTriggerName(parameter, this.nameResolver, attribute.MethodName);

            return Task.FromResult<ITriggerBinding?>(
                new DaprServiceInvocationTriggerBinding(this.serviceListener, methodName, parameter));
        }

        class DaprServiceInvocationTriggerBinding : DaprTriggerBindingBase
        {
            readonly DaprServiceListener serviceListener;
            readonly string methodName;

            public DaprServiceInvocationTriggerBinding(
                DaprServiceListener serviceListener,
                string methodName,
                ParameterInfo parameter)
                : base(serviceListener, parameter)
            {
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            }

            protected override DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor)
            {
                return new DaprServiceInvocationListener(this.serviceListener, executor, this.methodName);
            }

            sealed class DaprServiceInvocationListener : DaprListenerBase
            {
                readonly ITriggeredFunctionExecutor executor;
                readonly string methodName;

                public DaprServiceInvocationListener(
                    DaprServiceListener serviceListener,
                    ITriggeredFunctionExecutor executor,
                    string methodName)
                    : base(serviceListener)
                {
                    this.executor = executor;
                    this.methodName = methodName;
                }

                public override void Dispose()
                {
                    // no-op
                }

                public override void AddRoute(IRouteBuilder routeBuilder)
                {
                    routeBuilder.MapPost(this.methodName, this.DispatchAsync);
                }

                internal override async Task DispatchInternalAsync(HttpContext context)
                {
                    var input = new TriggeredFunctionData
                    {
                        TriggerValue = context,
                    };

                    FunctionResult result = await this.executor.TryExecuteAsync(input, context.RequestAborted);
                    if (!result.Succeeded)
                    {
                        throw result.Exception;
                    }
                }
            }
        }
    }
}