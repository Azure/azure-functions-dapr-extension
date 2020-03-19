// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>
    /// Defines the configuration options for the Dapr binding.
    /// </summary>
    [Extension("Dapr")]
    class DaprExtensionConfigProvider : IExtensionConfigProvider
    {
        readonly ILoggerFactory loggerFactory;
        readonly DaprServiceClient daprClient;     // TODO: Use an interface for mocking
        readonly DaprServiceListener daprListener; // TODO: Use an interface for mocking
        ILogger logger;

        public DaprExtensionConfigProvider(
            ILoggerFactory loggerFactory,
            DaprServiceClient daprClient,
            DaprServiceListener daprListener)
        {
            this.loggerFactory = loggerFactory ?? throw new NotImplementedException(nameof(loggerFactory));
            this.daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            this.daprListener = daprListener ?? throw new ArgumentNullException(nameof(daprListener));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.logger = this.loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
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
        }

        internal static SaveStateParameters CreateSaveStateParameters(JObject parametersJson)
        {
            var options = new SaveStateParameters()
            {
                StateStore = GetValueOrDefault<string>(parametersJson, "stateStore"),
                Key = GetValueOrDefault<string>(parametersJson, "key"),
                Value = GetValueOrDefault<JToken>(parametersJson, "value"),
            };

            return options;
        }

        internal static SaveStateParameters CreateSaveStateParameters(string parametersValue)
        {
            var options = new SaveStateParameters()
            {
                Value = parametersValue,
            };

            return options;
        }

        // TODO: Review this - it doesn't seem right to assume these are JSON bytes.
        //       Instead we should probably save the raw bytes as-is.
        //       More discussion: https://github.com/dapr/dapr/issues/235
        internal static SaveStateParameters CreateSaveStateParameters(byte[] parametersJsonBytes)
        {
            var options = new SaveStateParameters();
            try
            {
                string content = Encoding.UTF8.GetString(parametersJsonBytes);
                JObject jObject = JObject.Parse(content);
                options = CreateSaveStateParameters(jObject);
                if (options.Value == null)
                {
                    throw new FormatException("Invalid save state parameters JSON");
                }
            }
            catch (JsonException)
            {
                options.Value = parametersJsonBytes;
            }

            return options;
        }

        internal static InvokeMethodParameters CreateInvokeMethodParameters(JObject parametersJson)
        {
            var options = new InvokeMethodParameters()
            {
                AppId = GetValueOrDefault<string>(parametersJson, "appId"),
                MethodName = GetValueOrDefault<string>(parametersJson, "methodName"),
                Body = GetValueOrDefault<JToken>(parametersJson, "body"),
                HttpVerb = GetValueOrDefault<string>(parametersJson, "httpVerb"),
            };

            return options;
        }

        static TValue GetValueOrDefault<TValue>(JObject messageObject, string propertyName)
        {
            if (messageObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken result))
            {
                return result.Value<TValue>();
            }

            return default;
        }
    }
}