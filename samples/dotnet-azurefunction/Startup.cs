// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

[assembly: FunctionsStartup(typeof(Microsoft.Azure.WebJobs.Extensions.Dapr.Services.Startup))]
namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Services
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, ConsoleLoggerProvider>();
        }
    }
}