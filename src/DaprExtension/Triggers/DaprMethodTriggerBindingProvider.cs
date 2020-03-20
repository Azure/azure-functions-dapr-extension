// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    class DaprMethodTriggerBindingProvider : ITriggerBindingProvider
    {
        static readonly Task<ITriggerBinding> NotApplicableResult = Task.FromResult<ITriggerBinding>(null);

        readonly DaprServiceListener serviceListener;

        public DaprMethodTriggerBindingProvider(DaprServiceListener serviceListener)
        {
            this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            ParameterInfo parameter = context.Parameter;
            DaprMethodTriggerAttribute attribute =
                parameter.GetCustomAttribute<DaprMethodTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return NotApplicableResult;
            }

            string methodName = attribute.MethodName;
            if (string.IsNullOrEmpty(methodName))
            {
                MemberInfo method = parameter.Member;
                methodName = method.GetCustomAttribute<FunctionNameAttribute>()?.Name ?? method.Name;
            }

            return Task.FromResult<ITriggerBinding>(
                new DaprMethodTriggerBinding(this.serviceListener, methodName, parameter));
        }

        class DaprMethodTriggerBinding : ITriggerBinding
        {
            static readonly IReadOnlyDictionary<string, Type> StaticBindingContract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // This binding supports return values of any type
                { "$return", typeof(object).MakeByRefType() },
            };

            readonly DaprServiceListener serviceListener;
            readonly string methodName;
            readonly ParameterInfo parameter;

            public DaprMethodTriggerBinding(
                DaprServiceListener serviceListener,
                string methodName,
                ParameterInfo parameter)
            {
                this.serviceListener = serviceListener ?? throw new ArgumentNullException(nameof(serviceListener));
                this.methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
                this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            }

            public Type TriggerValueType => typeof(HttpContext);

            public IReadOnlyDictionary<string, Type> BindingDataContract => StaticBindingContract;

            public async Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                HttpContext requestContext = (HttpContext)value;
                Stream inputStream = requestContext.Request.Body;
                Type destinationType = this.parameter.ParameterType;

                var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                object convertedValue;
                if (inputStream == null)
                {
                    convertedValue = this.parameter.DefaultValue;
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
                        convertedValue = jsonValue.ToObject(destinationType);
                    }

                    // Support for complex expression binding
                    bindingData["content"] = jsonValue;
                }

                // Support for basic expression binding
                bindingData[this.parameter.Name] = convertedValue;

                var valueProvider = new ObjectValueProvider(convertedValue, destinationType);
                return new TriggerData(valueProvider, bindingData)
                {
                    ReturnValueProvider = new HttpContextReturnValueBinder(requestContext, destinationType),
                };
            }

            public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
            {
                IListener listener = new DaprMethodListener(
                    this.serviceListener,
                    context.Executor,
                    this.methodName);

                return Task.FromResult(listener);
            }

            public ParameterDescriptor ToParameterDescriptor()
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

    // TODO: Find a better home for this
    // TODO: Derive from some abstract base class
    class DaprMethodListener : IListener
    {
        readonly DaprServiceListener serviceListener;
        readonly ITriggeredFunctionExecutor executor;

        public DaprMethodListener(
            DaprServiceListener serviceListener,
            ITriggeredFunctionExecutor executor,
            string methodName)
        {
            this.serviceListener = serviceListener;
            this.executor = executor;

            this.ListenPath = "/" + methodName;
        }

        public PathString ListenPath { get; }

        Task IListener.StartAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.RegisterListenerAsync(this, cancellationToken);
        }

        Task IListener.StopAsync(CancellationToken cancellationToken)
        {
            return this.serviceListener.DeregisterListenerAsync(this, cancellationToken);
        }

        void IListener.Cancel()
        {
            // no-op
        }

        void IDisposable.Dispose()
        {
            // no-op
        }

        public async Task DispatchAsync(HttpContext context)
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
