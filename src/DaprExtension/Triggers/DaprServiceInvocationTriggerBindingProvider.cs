// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    class DaprServiceInvocationTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;

        public DaprServiceInvocationTriggerBindingProvider(DaprServiceListener serviceListener)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprServiceInvocationTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Utils.NullTriggerBindingTask;
            }

            string? methodName = attribute.MethodName;
            if (methodName == null)
            {
                MemberInfo method = parameter.Member;
                methodName = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? method.Name;
            }

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

                public override void AddRoutes(TriggerRouteHandler routeHandler)
                {
                    routeHandler.AddRoute(HttpMethod.Post, this.methodName, this.DispatchAsync);
                }

                public override void DeleteRoutes(TriggerRouteHandler routeHandler)
                {
                    routeHandler.DeleteRoute(HttpMethod.Post, this.methodName);
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
                    }
                    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                    {
                        // The request was aborted by the client. No-op.
                        // TODO: Consider moving exception handling into base class
                    }
                    catch (Exception)
                    {
                        // This means an unhandled exception occurred in the Functions runtime.
                        // This is often caused by the host shutting down while a function is still executing.
                        // TODO: Handle failure
                    }
                }
            }
        }
    }
}
