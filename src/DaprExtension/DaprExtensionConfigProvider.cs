﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the configuration options for the Dapr binding.
    /// </summary>
    [Extension("Dapr")]
    class DaprExtensionConfigProvider : IExtensionConfigProvider
    {
        readonly DaprServiceClient daprClient;     // TODO: Use an interface for mocking
        readonly DaprServiceListener daprListener; // TODO: Use an interface for mocking
        readonly ILogger logger;

        public DaprExtensionConfigProvider(
            DaprServiceClient daprClient,
            DaprServiceListener daprListener,
            ILoggerFactory loggerFactory)
        {
            this.daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            this.daprListener = daprListener ?? throw new ArgumentNullException(nameof(daprListener));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.logger.LogInformation($"Registered dapr extension");

            var daprStateConverter = new DaprStateConverter(this.daprClient);

            // NOTE: The order of conversions for each binding rules is important!
            var stateRule = context.AddBindingRule<DaprStateAttribute>();
            stateRule.AddConverter<JObject, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.AddConverter<object, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.BindToCollector(attr => new DaprSaveStateAsyncCollector(attr, this.daprClient));
            stateRule.BindToInput<string>(daprStateConverter);
            stateRule.BindToInput<JToken>(daprStateConverter);
            stateRule.BindToInput<JObject>(daprStateConverter);
            stateRule.BindToInput<Stream>(daprStateConverter);
            stateRule.BindToInput<byte[]>(daprStateConverter);

            // TODO: This does not work for nulls and value types. Need a better way of doing this conversion.
            stateRule.BindToInput<object?>(daprStateConverter);

            var invokeRule = context.AddBindingRule<DaprInvokeAttribute>();
            invokeRule.AddConverter<JObject, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.BindToCollector(attr => new DaprInvokeMethodAsyncCollector(attr, this.daprClient));

            var publishRule = context.AddBindingRule<DaprPublishAttribute>();
            publishRule.AddConverter<JObject, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<object, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.BindToCollector(attr => new DaprPublishAsyncCollector(attr, this.daprClient));

            var daprSecretConverter = new DaprSecretConverter(this.daprClient);
            var secretsRule = context.AddBindingRule<DaprSecretAttribute>();
            secretsRule.BindToInput<JObject>(daprSecretConverter);
            secretsRule.BindToInput<string?>(daprSecretConverter);
            secretsRule.BindToInput<byte[]>(daprSecretConverter);

            var daprActorStateConverter = new DaprActorStateConverter(this.daprClient);
            var actorStateRule = context.AddBindingRule<DaprActorStateAttribute>();
            actorStateRule.AddConverter<JObject, DaprActorStateRecord>(CreateSaveActorStateParameters);
            actorStateRule.AddConverter<object, DaprActorStateRecord>(CreateSaveActorStateParameters);
            actorStateRule.BindToCollector(attr => new DaprSaveActorStateAsyncCollector(attr, this.daprClient));
            actorStateRule.BindToInput<string>(daprActorStateConverter);
            actorStateRule.BindToInput<JToken>(daprActorStateConverter);
            actorStateRule.BindToInput<JObject>(daprActorStateConverter);
            actorStateRule.BindToInput<Stream>(daprActorStateConverter);
            actorStateRule.BindToInput<byte[]>(daprActorStateConverter);

            context.AddBindingRule<DaprMethodTriggerAttribute>()
                .BindToTrigger(new DaprMethodTriggerBindingProvider(this.daprListener));

            context.AddBindingRule<DaprTopicTriggerAttribute>()
                .BindToTrigger(new DaprTopicTriggerBindingProvider(this.daprListener));
        }

        static DaprPubSubEvent CreatePubSubEvent(object arg)
        {
            return new DaprPubSubEvent(JToken.FromObject(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(JObject json)
        {
            DaprPubSubEvent e = json.ToObject<DaprPubSubEvent>();
            if (e.Payload == null)
            {
                throw new ArgumentException($"A '{nameof(e.Payload).ToLowerInvariant()}' parameter is required for outbound pub/sub operations.", nameof(json));
            }

            return e;
        }

        internal static DaprStateRecord CreateSaveStateParameters(JObject parametersJson)
        {
            if (!TryGetValue(parametersJson, "value", out string? value))
            {
                throw new ArgumentException("A 'value' parameter is required for save-state operations.", nameof(parametersJson));
            }

            var parameters = new DaprStateRecord(value);

            if (TryGetValue(parametersJson, "key", out string? key))
            {
                parameters.Key = key;
            }

            return parameters;
        }

        internal static DaprStateRecord CreateSaveStateParameters(object parametersValue)
        {
            return new DaprStateRecord(JToken.FromObject(parametersValue));
        }

        internal static DaprActorStateRecord CreateSaveActorStateParameters(JObject parametersJson)
        {
            if (!TryGetValue(parametersJson, "value", out JToken? value))
            {
                throw new ArgumentException("A 'value' parameter is required for save actor state operations.", nameof(parametersJson));
            }

            var parameter = new DaprActorStateRecord(value);

            if (TryGetValue(parametersJson, "key", out string? key))
            {
                parameter.Key = key;
            }

            return parameter;
        }

        internal static DaprActorStateRecord CreateSaveActorStateParameters(object parametersValue)
        {
            return new DaprActorStateRecord(JToken.FromObject(parametersValue));
        }

        internal static DaprActorStateRecord CreateSaveActorStateParameters(JToken parametersValue)
        {
            return new DaprActorStateRecord(parametersValue);
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(JObject parametersJson)
        {
            var options = new InvokeMethodParameters();

            if (TryGetValue(parametersJson, "appId", out string? appId))
            {
                options.AppId = appId;
            }

            if (TryGetValue(parametersJson, "methodName", out string? methodName))
            {
                options.MethodName = methodName;
            }

            if (TryGetValue(parametersJson, "body", out JToken? body))
            {
                options.Body = body;
            }

            if (TryGetValue(parametersJson, "httpVerb", out string? httpVerb) && httpVerb != null)
            {
                options.HttpVerb = httpVerb;
            }

            return options;
        }

        static bool TryGetValue<TValue>(JObject messageObject, string propertyName, out TValue? value)
            where TValue : class
        {
            if (messageObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken result))
            {
                value = result.Value<TValue>();
                return true;
            }

            value = default;
            return false;
        }
    }
}