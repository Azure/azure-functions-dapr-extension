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
    using Microsoft.Extensions.Logging;

    class DaprServiceInvocationTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly ILogger logger;
        readonly IDaprServiceListener serviceListener;
        readonly INameResolver nameResolver;

        public DaprServiceInvocationTriggerBindingProvider(ILogger logger, IDaprServiceListener serviceListener, INameResolver resolver)
        {
            this.logger = logger;
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
                new DaprTriggerBindingWrapper(
                    new DaprServiceInvocationTriggerBinding(this.logger, this.serviceListener, methodName, parameter)));
        }

        class DaprServiceInvocationTriggerBinding : DaprTriggerBindingBase
        {
            readonly ILogger logger;
            readonly IDaprServiceListener serviceListener;
            readonly string methodName;

            public DaprServiceInvocationTriggerBinding(
                ILogger logger,
                IDaprServiceListener serviceListener,
                string methodName,
                ParameterInfo parameter)
                : base(serviceListener, parameter)
            {
                this.logger = logger;
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            }

            protected override DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor)
            {
                return new DaprServiceInvocationListener(this.logger, this.serviceListener, executor, this.methodName);
            }

            sealed class DaprServiceInvocationListener : DaprListenerBase
            {
                readonly ITriggeredFunctionExecutor executor;
                readonly string methodName;

                public DaprServiceInvocationListener(
                    ILogger logger,
                    IDaprServiceListener serviceListener,
                    ITriggeredFunctionExecutor executor,
                    string methodName)
                    : base(serviceListener)
                {
                    this.Logger = logger;
                    this.executor = executor;
                    this.methodName = methodName;
                }

                public override ILogger Logger { get; }

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