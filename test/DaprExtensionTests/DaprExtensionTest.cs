// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
    public abstract class DaprExtensionTest : IDisposable, IAsyncLifetime
    {
        static readonly HttpClient HttpClient = new HttpClient();

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

        internal IEnumerable<LogEntry> GetExtensionLogs()
        {
            string category = "Host.Triggers.Dapr";
            bool loggerExists = this.logProvider.TryGetLogs(category, out IEnumerable<LogEntry> logs);
            Assert.True(loggerExists, $"No logger was found for {category}. Did the extension get loaded?");

            return logs!;
        }

        internal async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string url,
            object jsonContent = null)
        {
            using var request = new HttpRequestMessage(method, url);

            if (jsonContent != null)
            {
                string json = JsonConvert.SerializeObject(jsonContent);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await HttpClient.SendAsync(request);
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
