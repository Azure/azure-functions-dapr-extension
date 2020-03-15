// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(DaprWebJobsStartup))]

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    /// <summary>
    /// Startup configuration class for the Dapr extension.
    /// </summary>
    public class DaprWebJobsStartup : IWebJobsStartup
    {
        /// <summary>
        /// Adds the Dapr extension to the WebJobs builder.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDapr();
        }
    }
}
