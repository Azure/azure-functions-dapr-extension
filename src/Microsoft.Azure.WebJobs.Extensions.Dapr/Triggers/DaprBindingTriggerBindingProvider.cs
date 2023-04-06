﻿// ------------------------------------------------------------
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

    class DaprBindingTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;
        readonly INameResolver nameResolver;

        public DaprBindingTriggerBindingProvider(DaprServiceListener serviceListener, INameResolver nameResolver)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
            this.nameResolver = nameResolver;
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprBindingTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return BindingUtils.NullTriggerBindingTask;
            }

            string daprBindingName = TriggerHelper.ResolveTriggerName(parameter, this.nameResolver, attribute.BindingName);

            return Task.FromResult<ITriggerBinding?>(
                new DaprTriggerBinding(this.serviceListener, daprBindingName, parameter));
        }

        class DaprTriggerBinding : DaprTriggerBindingBase
        {
            readonly DaprServiceListener serviceListener;
            readonly string bindingName;

            public DaprTriggerBinding(
                DaprServiceListener serviceListener,
                string daprBindingName,
                ParameterInfo parameter)
                : base(serviceListener, parameter)
            {
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.bindingName = daprBindingName ?? throw new ArgumentNullException(nameof(daprBindingName));
            }

            protected override DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor)
            {
                return new DaprTriggerListener(this.serviceListener, executor, this.bindingName);
            }

            sealed class DaprTriggerListener : DaprListenerBase
            {
                readonly ITriggeredFunctionExecutor executor;
                readonly string bindingName;

                public DaprTriggerListener(
                    DaprServiceListener serviceListener,
                    ITriggeredFunctionExecutor executor,
                    string bindingName)
                    : base(serviceListener)
                {
                    this.executor = executor;
                    this.bindingName = bindingName;
                }

                public override void Dispose()
                {
                    // no-op
                }

                public override void AddRoute(IRouteBuilder routeBuilder)
                {
                    routeBuilder.MapPost(this.bindingName, this.DispatchAsync);
                    routeBuilder.MapVerb("OPTIONS", this.bindingName, this.Success);
                }

                public async Task Success(HttpContext context)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(string.Empty);
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