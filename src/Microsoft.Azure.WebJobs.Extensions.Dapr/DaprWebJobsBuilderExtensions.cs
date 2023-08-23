// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for Dapr integration.
    /// </summary>
    public static class DaprWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds the Dapr extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <returns>Returns the updated webjobs builder.</returns>
        public static IWebJobsBuilder AddDapr(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var serviceProvider = builder.Services.BuildServiceProvider();
            var nameResolver = serviceProvider.GetRequiredService<INameResolver>();

            if (!EnvironmentExtensions.ShouldRegisterDaprExtension(nameResolver))
            {
                return builder;
            }

            builder.AddExtension<DaprExtensionConfigProvider>()
                .Services
                .AddSingleton<IDaprServiceClient, DaprServiceClient>()
                .AddSingleton<IDaprServiceListener, DaprServiceListener>()
                .AddSingleton<IDaprClient, DaprHttpClient>()
                .AddHttpClient();

            return builder;
        }
    }
}