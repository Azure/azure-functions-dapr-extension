// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using CloudNative.CloudEvents;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Triggers;
    using Newtonsoft.Json.Linq;

    class DaprTopicTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;

        public DaprTopicTriggerBindingProvider(DaprServiceListener serviceListener)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprTopicTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Utils.NullTriggerBindingTask;
            }

            string? topic = attribute.Topic;
            if (topic == null)
            {
                MemberInfo method = parameter.Member;
                topic = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? method.Name;
            }

            // Verison 0.8 only support route to be the same as the topic name
            return Task.FromResult<ITriggerBinding?>(
                new DaprTopicTriggerBinding(this.serviceListener, topic, route: topic, parameter));
        }

        class DaprTopicTriggerBinding : DaprTriggerBindingBase
        {
            static readonly JsonEventFormatter CloudEventFormatter = new JsonEventFormatter();

            readonly DaprServiceListener serviceListener;
            readonly string topicName;
            readonly string route;

            public DaprTopicTriggerBinding(
                DaprServiceListener serviceListener,
                string topicName,
                string route,
                ParameterInfo parameter)
                : base(serviceListener, parameter)
            {
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.topicName = topicName ?? throw new ArgumentNullException(nameof(topicName));
                this.route = route ?? throw new ArgumentNullException(nameof(route));
            }

            protected override DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor)
            {
                return new DaprTopicListener(this.serviceListener, executor, new DaprTopicSubscription(this.topicName, this.route));
            }

            protected override object ConvertFromJson(JToken jsonValue, Type destinationType)
            {
                // The input is always expected to be an object in the Cloud Events schema
                // https://github.com/cloudevents/spec/blob/v1.0/spec.md#example
                if (jsonValue is JObject jsonObject)
                {
                    if (destinationType == typeof(CloudEvent))
                    {
                        return CloudEventFormatter.DecodeJObject(jsonObject);
                    }
                    else if (jsonObject.TryGetValue("data", StringComparison.Ordinal, out JToken eventData))
                    {
                        // Do the generic conversion from the "data" payload
                        return base.ConvertFromJson(eventData, destinationType);
                    }
                }

                return base.ConvertFromJson(jsonValue, destinationType);
            }

            sealed class DaprTopicListener : DaprListenerBase
            {
                readonly ITriggeredFunctionExecutor executor;
                readonly DaprTopicSubscription topic;

                public DaprTopicListener(
                    DaprServiceListener serviceListener,
                    ITriggeredFunctionExecutor executor,
                    DaprTopicSubscription topic)
                    : base(serviceListener)
                {
                    this.executor = executor;
                    this.topic = topic;

                    serviceListener.RegisterTopic(this.topic);
                }

                public override void Dispose()
                {
                    // no-op
                }

                public override void AddRoute(IRouteBuilder routeBuilder)
                {
                    // Example: POST /orders
                    // { "topic": "newOrder", "route": "/orders"}
                    // https://github.com/dapr/docs/blob/master/reference/api/pubsub_api.md#provide-routes-for-dapr-to-deliver-topic-events
                    routeBuilder.MapPost(this.topic.Route, this.DispatchAsync);
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
