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
        }
    }
}