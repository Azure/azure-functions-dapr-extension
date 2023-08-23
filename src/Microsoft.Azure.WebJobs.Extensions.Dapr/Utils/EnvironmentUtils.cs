// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr.Utils
{
    using System;
    using Microsoft.Azure.WebJobs;

    internal static class EnvironmentUtils
    {
        public static bool IsAppService(INameResolver nameResolver)
        {
            return !string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId));
        }

        public static bool IsWindowsElasticPremium(INameResolver nameResolver)
        {
            string value = nameResolver.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku);
            return string.Equals(value, Constants.HostingPlanSkuConstants.ElasticPremiumSku, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldRegisterDaprExtension(INameResolver nameResolver)
        {
            return !IsWindowsElasticPremium(nameResolver) && !IsAppService(nameResolver);
        }
    }
}