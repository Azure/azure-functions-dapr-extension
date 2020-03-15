// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
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

            builder.AddExtension<DaprExtensionConfigProvider>()
                .Services
                .AddSingleton<DaprService>()
                .AddHttpClient();
            return builder;
        }
    }
}
