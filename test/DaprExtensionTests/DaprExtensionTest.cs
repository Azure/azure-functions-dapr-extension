// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DaprExtensionTests.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace DaprExtensionTests
{
    // TODO: Instead of making all tests run sequentially, configure a different port number for each collection
    [Collection("Sequential")]
    public abstract class DaprExtensionTest : IDisposable, IAsyncLifetime
    {
        static readonly HttpClient HttpClient = new HttpClient()
        {
            Timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10),
        };

        readonly TestLogProvider logProvider;
        readonly TestFunctionTypeLocator typeLocator;

        public DaprExtensionTest(ITestOutputHelper output)
        {
            this.logProvider = new TestLogProvider(output);
            this.typeLocator = new TestFunctionTypeLocator();

            this.Host = new HostBuilder()
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(this.logProvider))
                .ConfigureWebJobs(webJobsBuilder => webJobsBuilder.AddDapr())
                .ConfigureServices(
                    collection =>
                    {
                        collection.AddSingleton<ITypeLocator>(this.typeLocator);
                    })
                .Build();
        }

        protected IHost Host { get; }

        public virtual void Dispose()
        {
            this.Host.Dispose();
        }

        internal void AddFunctions(Type functionType) => this.typeLocator.AddFunctionType(functionType);

        internal IEnumerable<string> GetExtensionLogs()
        {
            return this.GetLogs("Host.Triggers.Dapr");
        }

        internal IEnumerable<string> GetFunctionLogs(string functionName)
        {
            return this.GetLogs($"Function.{functionName}.User");
        }

        internal IEnumerable<string> GetLogs(string category)
        {
            bool loggerExists = this.logProvider.TryGetLogs(category, out IEnumerable<LogEntry> logs);
            Assert.True(loggerExists, $"No logger was found for '{category}'.");

            return logs.Select(entry => entry.Message).ToArray();
        }

        internal async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string url,
            object? jsonContent = null)
        {
            using var request = new HttpRequestMessage(method, url);

            if (jsonContent != null)
            {
                string json = JsonConvert.SerializeObject(jsonContent);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await HttpClient.SendAsync(request);
        }

        internal Task CallFunctionAsync(string name, IDictionary<string, object>? args = null)
        {
            IJobHost jobHost = this.Host.Services.GetService<IJobHost>();
            return jobHost.CallAsync(name, args);
        }

        Task IAsyncLifetime.InitializeAsync() => this.Host.StartAsync();

        Task IAsyncLifetime.DisposeAsync() => this.Host.StopAsync();

        class TestFunctionTypeLocator : ITypeLocator
        {
            readonly List<Type> functionTypes = new List<Type>();

            public void AddFunctionType(Type functionType) => this.functionTypes.Add(functionType);

            IReadOnlyList<Type> ITypeLocator.GetTypes() => this.functionTypes.AsReadOnly();
        }
    }
}
