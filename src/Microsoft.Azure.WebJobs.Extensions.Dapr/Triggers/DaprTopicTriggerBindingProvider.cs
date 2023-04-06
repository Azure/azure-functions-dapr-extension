// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CloudNative.CloudEvents;
    using CloudNative.CloudEvents.SystemTextJson;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Triggers;

    class DaprTopicTriggerBindingProvider : ITriggerBindingProvider
    {
        readonly DaprServiceListener serviceListener;
        readonly INameResolver nameResolver;

        public DaprTopicTriggerBindingProvider(DaprServiceListener serviceListener, INameResolver nameResolver)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
            this.nameResolver = nameResolver;
        }

        public Task<ITriggerBinding?> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<DaprTopicTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return BindingUtils.NullTriggerBindingTask;
            }

            // Resolve names in pub/sub, topic, and route from settings
            if (!this.nameResolver.TryResolveWholeString(attribute.PubSubName, out var pubSubName))
            {
                pubSubName = attribute.PubSubName;
            }

            string topic = TriggerHelper.ResolveTriggerName(parameter, this.nameResolver, attribute.Topic);

            if (attribute.Route is null || !this.nameResolver.TryResolveWholeString(attribute.Route, out var route))
            {
                route = attribute.Route ?? topic;
            }

            if (!route.StartsWith("/"))
            {
                route = "/" + route;
            }

            return Task.FromResult<ITriggerBinding?>(
                new DaprTopicTriggerBinding(this.serviceListener, pubSubName, topic, route, parameter));
        }

        class DaprTopicTriggerBinding : DaprTriggerBindingBase
        {
            static readonly JsonEventFormatter CloudEventFormatter = new JsonEventFormatter();

            readonly DaprServiceListener serviceListener;
            readonly string pubSubName;
            readonly string topic;
            readonly string route;

            public DaprTopicTriggerBinding(
                DaprServiceListener serviceListener,
                string pubSubName,
                string topic,
                string route,
                ParameterInfo parameter)
                : base(serviceListener, parameter)
            {
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.pubSubName = pubSubName ?? throw new ArgumentNullException(nameof(pubSubName));
                this.topic = topic ?? throw new ArgumentNullException(nameof(topic));
                this.route = route ?? throw new ArgumentNullException(nameof(route));
            }

            protected override DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor)
            {
                return new DaprTopicListener(this.serviceListener, executor, new DaprTopicSubscription(this.pubSubName, this.topic, this.route));
            }

            protected override object ConvertFromJson(JsonElement jsonElement, Type destinationType)
            {
                // The input is always expected to be an object in the Cloud Events schema
                // https://github.com/cloudevents/spec/blob/v1.0/spec.md#example
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    if (destinationType == typeof(CloudEvent))
                    {
                        return CloudEventFormatter.ConvertFromJsonElement(jsonElement, null);
                    }
                    else if (jsonElement.TryGetProperty("data", out JsonElement eventData))
                    {
                        // Do the generic conversion from the "data" payload
                        return JsonSerializer.Deserialize(eventData.GetRawText(), destinationType)
                            ?? throw new InvalidOperationException("Failed to deserialize event data");
                    }
                }

                return base.ConvertFromJson(jsonElement, destinationType);
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
                    // { "pubsubname": "pubsub", "topic": "newOrder", "route": "/orders"}
                    // https://docs.dapr.io/reference/api/pubsub_api/#provide-routes-for-dapr-to-deliver-topic-events
                    routeBuilder.MapPost(this.topic.Route, this.DispatchAsync);
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