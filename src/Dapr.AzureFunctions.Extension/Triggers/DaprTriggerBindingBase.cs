// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.AzureFunctions.Extension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Executors;
    using Microsoft.Azure.WebJobs.Host.Listeners;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Microsoft.Azure.WebJobs.Host.Triggers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    abstract class DaprTriggerBindingBase : ITriggerBinding
    {
        readonly DaprServiceListener serviceListener;
        readonly ParameterInfo parameter;

        public DaprTriggerBindingBase(DaprServiceListener serviceListener, ParameterInfo parameter)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));

            this.BindingDataContract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Allow return values of any type
                { "$return", typeof(object).MakeByRefType() },

                // Allow binding to the name of the parameter in binding expressions
                { parameter.Name, parameter.ParameterType },
            };
        }

        // All Dapr triggers are HTTP-based
        public Type TriggerValueType => typeof(HttpContext);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; }

        protected abstract DaprListenerBase OnCreateListener(ITriggeredFunctionExecutor executor);

        async Task<ITriggerData> ITriggerBinding.BindAsync(object value, ValueBindingContext context)
        {
            HttpContext requestContext = (HttpContext)value;
            Stream inputStream = requestContext.Request.Body;
            Type destinationType = this.parameter.ParameterType;

            var bindingData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            object? convertedValue;
            if (inputStream == null || requestContext.Request.ContentLength == 0)
            {
                // Assigns null for reference types or the default value for value types
                if (this.parameter.ParameterType.IsValueType)
                {
                    convertedValue = Activator.CreateInstance(this.parameter.ParameterType);
                }
                else
                {
                    convertedValue = null;
                }
            }
            else if (destinationType.IsAssignableFrom(inputStream.GetType()))
            {
                // Give them the request stream directly - no buffering
                convertedValue = inputStream;
            }
            else if (destinationType.IsAssignableFrom(typeof(byte[])))
            {
                // Copy the stream into a simple byte array with the help of MemoryStream
                using (var buffer = new MemoryStream((int?)requestContext.Request.ContentLength ?? 4096))
                using (inputStream)
                {
                    await inputStream.CopyToAsync(buffer);
                    convertedValue = buffer.ToArray();
                }
            }
            else
            {
                // Binding to JToken or some derivative like JObject or JArray.
                // This also works for primitives like int, bool, and string.
                JToken jsonValue;
                using (var reader = new JsonTextReader(new StreamReader(inputStream)))
                {
                    jsonValue = await JToken.ReadFromAsync(reader, context.CancellationToken);
                }

                if (destinationType.IsAssignableFrom(jsonValue.GetType()))
                {
                    convertedValue = jsonValue;
                }
                else if (destinationType == typeof(string) && jsonValue.Type != JTokenType.String)
                {
                    // Special case for out-of-proc workers (like nodejs). The binding type
                    // appears to always be "string" so we need to do a special conversion.
                    convertedValue = jsonValue.ToString(Formatting.None);
                }
                else
                {
                    // At this point, we're probably dealing with a POCO
                    convertedValue = this.ConvertFromJson(jsonValue, destinationType);
                }
            }

            // Support for basic expression binding
            bindingData[this.parameter.Name] = convertedValue;

            var valueProvider = new ObjectValueProvider(convertedValue, destinationType);
            return new TriggerData(valueProvider, bindingData)
            {
                ReturnValueProvider = new HttpContextReturnValueBinder(requestContext, destinationType),
            };
        }

        protected virtual object ConvertFromJson(JToken jsonValue, Type destinationType)
        {
            // Do a direct conversion by default
            return jsonValue.ToObject(destinationType)!;
        }

        Task<IListener> ITriggerBinding.CreateListenerAsync(ListenerFactoryContext context)
        {
            DaprListenerBase daprListener = this.OnCreateListener(context.Executor);
            this.serviceListener.AddFunctionListener(daprListener);
            return Task.FromResult<IListener>(daprListener);
        }

        ParameterDescriptor ITriggerBinding.ToParameterDescriptor()
        {
            return new ParameterDescriptor { Name = this.parameter.Name };
        }

        class ObjectValueProvider : IValueProvider
        {
            private readonly object? value;
            private readonly Task<object?> valueAsTask;

            public ObjectValueProvider(object? value, Type valueType)
            {
                if (value != null && !valueType.IsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException($"Cannot convert {value} to {valueType.Name}.");
                }

                this.value = value;
                this.valueAsTask = Task.FromResult(value);
                this.Type = valueType;
            }

            public Type Type { get; }

            public Task<object?> GetValueAsync() => this.valueAsTask;

            public string? ToInvokeString() => this.value?.ToString();
        }

        class HttpContextReturnValueBinder : IValueBinder
        {
            readonly HttpContext context;

            object? outputValue;

            public HttpContextReturnValueBinder(HttpContext context, Type valueType)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
                this.Type = valueType ?? throw new ArgumentNullException(nameof(valueType));
            }

            public Type Type { get; }

            public Task<object> GetValueAsync()
            {
                throw new NotImplementedException("This binder should only be used for setting return values!");
            }

            public async Task SetValueAsync(object? value, CancellationToken cancellationToken)
            {
                if (value == null)
                {
                    return;
                }

                this.outputValue = value;

                // Assume the response in JSON
                this.context.Response.ContentType = "application/json";
                if (value is Stream streamResult)
                {
                    using (streamResult)
                    {
                        await streamResult.CopyToAsync(this.context.Response.Body);
                    }
                }
                else if (value is byte[] bytes)
                {
                    await this.context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
                else
                {
                    string jsonResult = JsonConvert.SerializeObject(value, Formatting.None);
                    await this.context.Response.WriteAsync(jsonResult, cancellationToken);
                }
            }

            public string? ToInvokeString()
            {
                if (this.outputValue == null)
                {
                    return null;
                }

                try
                {
                    return JsonConvert.SerializeObject(this.outputValue);
                }
                catch (JsonException)
                {
                    return null; // best effort
                }
            }
        }
    }
}
