// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
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

            context.AddConverter<JObject, SaveStateParameters>(CreateSaveStateParameters);
            context.AddConverter<string, SaveStateParameters>(CreateSaveStateParameters);
            context.AddConverter<byte[], SaveStateParameters>(CreateSaveStateParameters);
            context.AddConverter<JObject, InvokeMethodParameters>(CreateInvokeMethodParameters);

            var daprStateConverter = new DaprStateConverter(this.daprClient);

            var stateRule = context.AddBindingRule<DaprStateAttribute>();
            stateRule.BindToInput<byte[]>(daprStateConverter);
            stateRule.BindToInput<string>(daprStateConverter);
            stateRule.BindToInput<Stream>(daprStateConverter);
            stateRule.BindToInput<JToken>(daprStateConverter);
            stateRule.BindToInput<JObject>(daprStateConverter);
            stateRule.BindToCollector(attr => new DaprSaveStateAsyncCollector(attr, this.daprClient));

            var invokeRule = context.AddBindingRule<DaprInvokeAttribute>();
            invokeRule.BindToCollector(attr => new DaprInvokeMethodAsyncCollector(attr, this.daprClient));

            context.AddBindingRule<DaprMethodTriggerAttribute>()
                .BindToTrigger(new DaprMethodTriggerBindingProvider(this.daprListener));

            context.AddBindingRule<DaprTopicTriggerAttribute>()
                .BindToTrigger(new DaprTopicTriggerBindingProvider(this.daprListener));
        }

        internal static SaveStateParameters CreateSaveStateParameters(JObject parametersJson)
        {
            if (!TryGetValue(parametersJson, "value", out string? value))
            {
                throw new ArgumentException("A 'value' parameter is required for save-state operations.", nameof(parametersJson));
            }

            var parameters = new SaveStateParameters(value);

            if (TryGetValue(parametersJson, "stateStore", out string? stateStore))
            {
                parameters.StateStore = stateStore;
            }

            if (!TryGetValue(parametersJson, "key", out string? key))
            {
                parameters.Key = key;
            }

            return parameters;
        }

        internal static SaveStateParameters CreateSaveStateParameters(object parametersValue)
        {
            return new SaveStateParameters(JToken.FromObject(parametersValue));
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