// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Microsoft.Azure.WebJobs.Extensions.Dapr
{
    internal static class Constants
    {
        public class EnvironmentKeys
        {
            public const string AppPort = "DAPR_APP_PORT";
            public const string DisableSidecarMetadataCheck = "DAPR_DISABLE_SIDECAR_METADATA_CHECK";
            public const string SidecarHttpPort = "DAPR_HTTP_PORT";
            public const string AzureWebsiteInstanceId = "WEBSITE_INSTANCE_ID";
            public const string AzureWebsiteSku = "WEBSITE_SKU";
        }

        public class HostingPlanSkuConstants
        {
            public const string ElasticPremiumSku = "ElasticPremium";
        }
    }
}