// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>
    /// Defines the configuration options for the Dapr binding.
    /// </summary>
    [Extension("Dapr", "Dapr")]
    internal class DaprExtensionConfigProvider : IExtensionConfigProvider
    {
        private ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly DaprService _daprService;

        public DaprExtensionConfigProvider(ILoggerFactory loggerFactory, DaprService daprService)
        {
            _loggerFactory = loggerFactory;
            _daprService = daprService;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _logger = _loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Dapr"));
            _logger.LogInformation($"Registered dapr extension");

            context.AddConverter<JObject, SaveStateOptions>(SaveStateOptions);
            context.AddConverter<JObject, InvokeMethodOptions>(InvokeMethodOptions);
            
            var daprStateConverter = new DaprStateConverter(_daprService);
            
            var stateRule = context.AddBindingRule<DaprStateAttribute>();
            stateRule.BindToInput<byte[]>(daprStateConverter);
            stateRule.BindToInput<string>(daprStateConverter);
            stateRule.BindToInput<Stream>(daprStateConverter);
            stateRule.BindToCollector<SaveStateOptions>((attr) => {
                return new DaprSaveStateAsyncCollector(attr, _daprService);
            });

            var invokeRule = context.AddBindingRule<DaprInvokeAttribute>();
            invokeRule.BindToCollector<InvokeMethodOptions>((attr) => {
                return new DaprInvokeMethodAsyncCollector(attr, _daprService);
            });
        }

        internal static SaveStateOptions SaveStateOptions(JObject saveStateOptions)
        {
            var options = new SaveStateOptions()
            {
                StateStore = GetValueOrDefault<string>(saveStateOptions, "stateStore"),
                Key = GetValueOrDefault<string>(saveStateOptions, "key"),
                Value = GetValueOrDefault<JToken>(saveStateOptions, "value")
            };

            return options;
        }

        internal static InvokeMethodOptions InvokeMethodOptions(JObject invokeMethodOptions)
        {
            var options = new InvokeMethodOptions()
            {
                AppId = GetValueOrDefault<string>(invokeMethodOptions, "appId"),
                MethodName = GetValueOrDefault<string>(invokeMethodOptions, "method"),
                Body = GetValueOrDefault<JToken>(invokeMethodOptions, "body")
            };

            return options;
        }

        private static TValue GetValueOrDefault<TValue>(JObject messageObject, string propertyName)
        {
            if (messageObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken result))
            {
                return result.Value<TValue>();
            }

            return default;
        }
    }

    public class InvokeMethodOptions
    {
        public string AppId { get; set; }
        public string MethodName { get; set; }
        public HttpMethod HttpVerb { get; set; }
        public JToken Body { get; set; }
    }

    public class SaveStateOptions
    {
        public string StateStore { get; set; }
        public string Key { get; set; }
        public JToken Value { get; set; }
    }
}
