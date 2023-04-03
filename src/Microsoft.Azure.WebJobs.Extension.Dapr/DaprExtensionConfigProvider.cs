// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extension.Dapr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using Microsoft.Azure.WebJobs;
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
        readonly INameResolver nameResolver;
        readonly ILogger logger;

        public DaprExtensionConfigProvider(
            DaprServiceClient daprClient,
            DaprServiceListener daprListener,
            ILoggerFactory loggerFactory,
            INameResolver nameResolver)
        {
            this.daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            this.daprListener = daprListener ?? throw new ArgumentNullException(nameof(daprListener));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
            this.nameResolver = nameResolver;
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
            stateRule.AddConverter<byte[], DaprStateRecord>(CreateSaveStateParameters);
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
            invokeRule.AddConverter<byte[], InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.AddConverter<JObject, InvokeMethodParameters>(CreateInvokeMethodParameters);
            invokeRule.BindToCollector(attr => new DaprInvokeMethodAsyncCollector(attr, this.daprClient));

            var publishRule = context.AddBindingRule<DaprPublishAttribute>();
            publishRule.AddConverter<byte[], DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<JObject, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.AddConverter<object, DaprPubSubEvent>(CreatePubSubEvent);
            publishRule.BindToCollector(attr => new DaprPublishAsyncCollector(attr, this.daprClient));

            var daprBindingRule = context.AddBindingRule<DaprBindingAttribute>();
            daprBindingRule.AddConverter<JsonElement, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<byte[], DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.AddConverter<object, DaprBindingMessage>(CreateBindingMessage);
            daprBindingRule.BindToCollector(attr => new DaprBindingAsyncCollector(attr, this.daprClient));

            var daprSecretConverter = new DaprSecretConverter(this.daprClient);
            var secretsRule = context.AddBindingRule<DaprSecretAttribute>();
            secretsRule.BindToInput<string?>(daprSecretConverter);
            secretsRule.BindToInput<byte[]>(daprSecretConverter);
            secretsRule.BindToInput<JObject>(daprSecretConverter);
            secretsRule.BindToInput<IDictionary<string, string>>(daprSecretConverter);

            context.AddBindingRule<DaprServiceInvocationTriggerAttribute>()
                .BindToTrigger(new DaprServiceInvocationTriggerBindingProvider(this.daprListener, this.nameResolver));

            context.AddBindingRule<DaprTopicTriggerAttribute>()
                .BindToTrigger(new DaprTopicTriggerBindingProvider(this.daprListener, this.nameResolver));

            context.AddBindingRule<DaprBindingTriggerAttribute>()
                .BindToTrigger(new DaprBindingTriggerBindingProvider(this.daprListener, this.nameResolver));
        }

        static DaprPubSubEvent CreatePubSubEvent(byte[] arg)
        {
            return CreatePubSubEvent(BytesToJObject(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(object arg)
        {
            return new DaprPubSubEvent(JToken.FromObject(arg));
        }

        static DaprPubSubEvent CreatePubSubEvent(JObject json)
        {
            DaprPubSubEvent? event_ = json.ToObject<DaprPubSubEvent>();
            if (event_ == null || event_.Payload == null)
            {
                throw new ArgumentException($"A '{nameof(event_.Payload).ToLowerInvariant()}' parameter is required for outbound pub/sub operations.", nameof(json));
            }

            return event_;
        }

        static JObject BytesToJObject(byte[] arg)
        {
            string json = Encoding.UTF8.GetString(arg);
            return JObject.Parse(json);
        }

        static DaprBindingMessage CreateBindingMessage(object paramValues)
        {
            return new DaprBindingMessage(paramValues);
        }

        static DaprBindingMessage CreateBindingMessage(byte[] paramValues)
        {
            var jsonDocument = JsonDocument.Parse(Encoding.UTF8.GetString(paramValues));
            return CreateBindingMessage(jsonDocument.RootElement);
        }

        static DaprBindingMessage CreateBindingMessage(JsonElement jsonElement)
        {
            if (!jsonElement.TryGetProperty("data", out JsonElement data))
            {
                throw new ArgumentException("A 'data' parameter is required for Dapr Binding operations.", nameof(jsonElement));
            }

            DaprBindingMessage message = new DaprBindingMessage(data!);

            if (jsonElement.TryGetProperty("operation", out JsonElement operation))
            {
                message.Operation = operation.GetRawText();
            }

            if (jsonElement.TryGetProperty("metadata", out JsonElement metadata))
            {
                message.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            }

            if (jsonElement.TryGetProperty("bindingName", out JsonElement binding))
            {
                message.BindingName = binding.GetRawText();
            }

            return message;
        }

        internal static DaprStateRecord CreateSaveStateParameters(byte[] arg)
        {
            return CreateSaveStateParameters(BytesToJObject(arg));
        }

        internal static DaprStateRecord CreateSaveStateParameters(JObject parametersJson)
        {
            if (!TryGetValue(parametersJson, "value", out JToken? value))
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

        internal static InvokeMethodParameters CreateInvokeMethodParameters(byte[] arg)
        {
            return CreateInvokeMethodParameters(BytesToJObject(arg));
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
            if (messageObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken? result))
            {
                value = result.Value<TValue>();
                return true;
            }

            value = default;
            return false;
        }
    }
}