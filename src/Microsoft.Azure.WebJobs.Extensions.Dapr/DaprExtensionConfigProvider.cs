// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Bindings.Converters;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the configuration options for the Dapr binding.
    /// </summary>
    [Extension("Dapr")]
    class DaprExtensionConfigProvider : IExtensionConfigProvider
    {
        readonly IDaprServiceClient daprClient;
        readonly IDaprServiceListener daprListener;
        readonly INameResolver nameResolver;
        readonly ILoggerFactory loggerFactory;
        readonly ILogger logger;

        public DaprExtensionConfigProvider(
            IDaprServiceClient daprClient,
            IDaprServiceListener daprListener,
            ILoggerFactory loggerFactory,
            INameResolver nameResolver)
        {
            this.daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            this.daprListener = daprListener ?? throw new ArgumentNullException(nameof(daprListener));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.nameResolver = nameResolver;

            this.logger = loggerFactory.CreateLogger(LoggingUtils.CreateDaprTriggerCategory());
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.logger.LogInformation($"Registered Dapr extension");

            var daprStateConverter = new DaprStateConverter(this.daprClient);

            // NOTE: The order of conversions for each binding rules is important!
            var stateRule = context.AddBindingRule<DaprStateAttribute>();
            stateRule.AddConverter<byte[], DaprStateRecord>(CreateSaveStateParameters);
            stateRule.AddConverter<JsonElement, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.AddConverter<JObject, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.AddConverter<JToken, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.AddConverter<object, DaprStateRecord>(CreateSaveStateParameters);
            stateRule.BindToCollector(attr => new DaprSaveStateAsyncCollector(attr, this.daprClient));
            stateRule.BindToInput<DaprStateRecord>(daprStateConverter);
            stateRule.BindToInput<byte[]>(daprStateConverter);
            stateRule.BindToInput<string>(daprStateConverter);
            stateRule.BindToInput<Stream>(daprStateConverter);
            stateRule.BindToInput<JsonElement>(daprStateConverter);
            stateRule.BindToInput<JObject>(daprStateConverter);
            stateRule.BindToInput<JToken>(daprStateConverter);
            stateRule.BindToInput<OpenType>(typeof(DaprStateGenericsConverter<>), this.daprClient);

            var invokeRule = context.AddBindingRule<DaprInvokeAttribute>();
            invokeRule.AddConverter<byte[], InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.AddConverter<JsonElement, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.AddConverter<JObject, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.AddConverter<JToken, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.AddConverter<object, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.BindToCollector(attr => new DaprInvokeMethodAsyncCollector(attr, this.daprClient));

            var publishRule = context.AddBindingRule<DaprPublishAttribute>();
            publishRule.AddConverter<byte[], DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<JsonElement, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<JObject, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<JToken, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<object, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.BindToCollector(attr => new DaprPublishAsyncCollector(attr, this.daprClient));

            var daprBindingRule = context.AddBindingRule<DaprBindingAttribute>();
            daprBindingRule.AddConverter<byte[], DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<JsonElement, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<JObject, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<JToken, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<object, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.BindToCollector(attr => new DaprBindingAsyncCollector(attr, this.daprClient));

            var daprSecretConverter = new DaprSecretConverter(this.daprClient);
            var secretsRule = context.AddBindingRule<DaprSecretAttribute>();
            secretsRule.BindToInput<byte[]>(daprSecretConverter);
            secretsRule.BindToInput<string?>(daprSecretConverter);
            secretsRule.BindToInput<JsonElement>(daprSecretConverter);
            secretsRule.BindToInput<JObject>(daprSecretConverter);
            secretsRule.BindToInput<JToken>(daprSecretConverter);
            secretsRule.BindToInput<OpenType>(typeof(DaprSecretsGenericsConverter<>), this.daprClient);

            context.AddBindingRule<DaprServiceInvocationTriggerAttribute>()
                .BindToTrigger(new DaprServiceInvocationTriggerBindingProvider(
                    this.loggerFactory.CreateLogger(LoggingUtils.CreateDaprTriggerCategory("ServiceInvocationTrigger")),
                    this.daprListener,
                    this.nameResolver));

            context.AddBindingRule<DaprTopicTriggerAttribute>()
                .BindToTrigger(new DaprTopicTriggerBindingProvider(
                    this.loggerFactory.CreateLogger(LoggingUtils.CreateDaprTriggerCategory("TopicTrigger")),
                    this.daprListener,
                    this.nameResolver));

            context.AddBindingRule<DaprBindingTriggerAttribute>()
                .BindToTrigger(new DaprBindingTriggerBindingProvider(
                    this.loggerFactory.CreateLogger(LoggingUtils.CreateDaprTriggerCategory("BindingTrigger")),
                    this.daprListener,
                    this.nameResolver));
        }

        static DaprPubSubEvent CreatePubSubEvent(byte[] arg)
        {
            return CreatePubSubEvent(BytesToJsonElement(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(object arg)
        {
            return new DaprPubSubEvent(arg);
        }

        static DaprPubSubEvent CreatePubSubEvent(JObject arg)
        {
            return CreatePubSubEvent(CreateJsonElementFromJObject(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(JToken arg)
        {
            return CreatePubSubEvent(CreateJsonElementFromJToken(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(JsonElement json)
        {
            if (!json.TryGetProperty("payload", out JsonElement payload))
            {
                throw new ArgumentException("A 'payload' parameter is required for outbound pub/sub operations.", nameof(json));
            }

            object? payloadObject = payload.Deserialize<object>();
            if (payloadObject == null)
            {
                throw new ArgumentException($"A '{nameof(payloadObject).ToLowerInvariant()}' parameter is required for outbound pub/sub operations.", nameof(json));
            }

            DaprPubSubEvent event_ = new DaprPubSubEvent(payloadObject);

            if (json.TryGetProperty("topic", out JsonElement topic))
            {
                event_.Topic = topic.GetString();
            }

            if (json.TryGetProperty("pubsubname", out JsonElement pubsubName))
            {
                event_.PubSubName = pubsubName.GetString();
            }

            return event_;
        }

        static JsonElement BytesToJsonElement(byte[] arg)
        {
            string json = Encoding.UTF8.GetString(arg);
            return JsonDocument.Parse(json).RootElement;
        }

        static DaprBindingMessage CreateBindingMessage(byte[] paramValues)
        {
            return CreateBindingMessage(BytesToJsonElement(paramValues));
        }

        static DaprBindingMessage CreateBindingMessage(object paramValues)
        {
            return new DaprBindingMessage(paramValues);
        }

        static DaprBindingMessage CreateBindingMessage(JObject arg)
        {
            return CreateBindingMessage(CreateJsonElementFromJObject(arg));
        }

        static DaprBindingMessage CreateBindingMessage(JToken arg)
        {
            return CreateBindingMessage(CreateJsonElementFromJToken(arg));
        }

        static DaprBindingMessage CreateBindingMessage(JsonElement jsonElement)
        {
            if (!jsonElement.TryGetProperty("data", out JsonElement data))
            {
                throw new ArgumentException("A 'data' parameter is required for Dapr Binding operations.", nameof(jsonElement));
            }

            object? dataObj = data.Deserialize<object>();
            if (dataObj == null)
            {
                throw new ArgumentException("Could not deserialize 'data' parameter for Dapr Binding operations.", nameof(jsonElement));
            }

            DaprBindingMessage message = new DaprBindingMessage(dataObj);

            if (jsonElement.TryGetProperty("operation", out JsonElement operation))
            {
                message.Operation = JsonSerializer.Deserialize<string>(operation);
            }

            if (jsonElement.TryGetProperty("metadata", out JsonElement metadata))
            {
                message.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            }

            if (jsonElement.TryGetProperty("bindingName", out JsonElement binding))
            {
                message.BindingName = JsonSerializer.Deserialize<string>(binding);
            }

            return message;
        }

        internal static DaprStateRecord CreateSaveStateParameters(byte[] arg)
        {
            return CreateSaveStateParameters(BytesToJsonElement(arg));
        }

        internal static DaprStateRecord CreateSaveStateParameters(JObject value)
        {
            return CreateSaveStateParameters(CreateJsonElementFromJObject(value));
        }

        internal static DaprStateRecord CreateSaveStateParameters(JToken value)
        {
            return CreateSaveStateParameters(CreateJsonElementFromJToken(value));
        }

        internal static DaprStateRecord CreateSaveStateParameters(JsonElement parametersJson)
        {
            if (!parametersJson.TryGetProperty("value", out JsonElement value))
            {
                throw new ArgumentException("A 'value' parameter is required for save-state operations.", nameof(parametersJson));
            }

            var parameters = new DaprStateRecord(value);

            if (parametersJson.TryGetProperty("key", out JsonElement key))
            {
                parameters.Key = key.GetString();
            }

            return parameters;
        }

        internal static DaprStateRecord CreateSaveStateParameters(object value)
        {
            return new DaprStateRecord(value);
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(byte[] arg)
        {
            return CreateInvokeMethodParameters(BytesToJsonElement(arg));
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(JObject arg)
        {
            return CreateInvokeMethodParameters(CreateJsonElementFromJObject(arg));
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(JToken arg)
        {
            return CreateInvokeMethodParameters(CreateJsonElementFromJToken(arg));
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(JsonElement parametersJson)
        {
            var options = new InvokeMethodParameters();

            if (parametersJson.TryGetProperty("appId", out JsonElement appId))
            {
                options.AppId = appId.GetRawText();
            }

            if (parametersJson.TryGetProperty("methodName", out JsonElement methodName))
            {
                options.MethodName = methodName.GetRawText();
            }

            if (parametersJson.TryGetProperty("body", out JsonElement body))
            {
                options.Body = body;
            }

            if (parametersJson.TryGetProperty("httpVerb", out JsonElement httpVerb))
            {
                options.HttpVerb = httpVerb.GetRawText();
            }

            return options;
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(object arg)
        {
            return new InvokeMethodParameters();
        }

        internal static JsonElement CreateJsonElementFromJObject(JObject obj)
        {
            return JsonDocument.Parse(obj.ToString()).RootElement;
        }

        internal static JsonElement CreateJsonElementFromJToken(JToken obj)
        {
            return JsonDocument.Parse(obj.ToString()).RootElement;
        }
    }
}