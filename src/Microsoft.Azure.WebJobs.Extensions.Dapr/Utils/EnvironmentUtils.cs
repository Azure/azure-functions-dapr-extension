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

        public static bool IsLinuxConsumptionOnLegion(INameResolver nameResolver)
        {
            return !IsAppService(nameResolver) &&
                   (!string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.ContainerName)) ||
                   !string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.WebsitePodName))) &&
                   !string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.LegionServiceHost));
        }

        public static bool IsFlexConsumptionSku(INameResolver nameResolver)
        {
            string value = nameResolver.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku);
            if (string.Equals(value, Constants.HostingPlanSkuConstants.FlexConsumptionSku, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // when in placeholder mode, site settings like SKU are not available
            // to enable this check to run in both modes, we check additional settings
            return IsLinuxConsumptionOnLegion(nameResolver);
        }

        public static bool IsLinuxConsumptionOnAtlas(INameResolver nameResolver)
        {
            return !IsAppService(nameResolver) &&
                   !string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.ContainerName)) &&
                   string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.LegionServiceHost));
        }

        public static bool IsManagedAppEnvironment(INameResolver nameResolver)
        {
            return !string.IsNullOrEmpty(nameResolver.Resolve(Constants.EnvironmentKeys.ManagedEnvironment));
        }

        public static bool IsAnyLinuxConsumption(INameResolver nameResolver)
        {
            return (IsLinuxConsumptionOnAtlas(nameResolver) || IsFlexConsumptionSku(nameResolver)) && !IsManagedAppEnvironment(nameResolver);
        }

        public static bool IsWindowsConsumption(INameResolver nameResolver)
        {
            string value = nameResolver.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku);
            return string.Equals(value, Constants.HostingPlanSkuConstants.DynamicSku, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsConsumptionSku(INameResolver nameResolver)
        {
            return IsWindowsConsumption(nameResolver) || IsAnyLinuxConsumption(nameResolver) || IsFlexConsumptionSku(nameResolver);
        }

        public static bool ShouldRegisterDaprExtension(INameResolver nameResolver)
        {
            return !IsWindowsElasticPremium(nameResolver) && !IsAppService(nameResolver) && !IsConsumptionSku(nameResolver);
        }
    }
}