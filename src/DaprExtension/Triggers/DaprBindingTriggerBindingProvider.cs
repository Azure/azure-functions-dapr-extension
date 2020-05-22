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
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    class DaprBindingTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;

        public DaprBindingTriggerBindingProvider(DaprServiceListener serviceListener)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprBindingTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Utils.NullTriggerBindingTask;
            }

            string? triggerName = attribute.BindingName;
            if (triggerName == null)
            {
                MemberInfo method = parameter.Member;
                triggerName = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? method.Name;
            }

            return Task.FromResult<ITriggerBinding?>(
                new DaprTriggerBinding(this.serviceListener, triggerName, parameter));
        }

        class DaprTriggerBinding : DaprTriggerBindingBase
        {
            readonly DaprServiceListener serviceListener;
            readonly string methodName;

            public DaprTriggerBinding(
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
                return new DaprTriggerListener(this.serviceListener, executor, this.methodName);
            }

            sealed class DaprTriggerListener : DaprListenerBase
            {
                readonly ITriggeredFunctionExecutor executor;
                readonly string triggerName;

                public DaprTriggerListener(
                    DaprServiceListener serviceListener,
                    ITriggeredFunctionExecutor executor,
                    string methodName)
                    : base(serviceListener)
                {
                    this.executor = executor;
                    this.triggerName = methodName;
                }

                public override void Dispose()
                {
                    // no-op
                }

                public override void AddRoute(IRouteBuilder routeBuilder)
                {
                    routeBuilder.MapPost(this.triggerName, this.DispatchAsync);
                    routeBuilder.MapVerb("OPTIONS", this.triggerName, this.Success);
                }

                public async Task Success(HttpContext context)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(string.Empty);
                }

                public override async Task DispatchAsync(HttpContext context)
                {
                    var input = new TriggeredFunctionData
                    {
                        TriggerValue = context,
                    };

                    try
                    {
                        FunctionResult result = await this.executor.TryExecuteAsync(input, context.RequestAborted);

                        // TODO: How do we handle failed function calls? We probably shouldn't 500, as they could retry indefinitely
                    }
                    catch (Exception)
                    {
                        // This means an unhandled exception occurred in the Functions runtime.
                        // This is often caused by the host shutting down while a function is still executing.
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(string.Empty);
                    }
                }
            }
        }
    }
}
