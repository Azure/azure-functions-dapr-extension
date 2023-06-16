namespace EndToEndTests.Framework
{
    public static class Constants
    {
        public const string DaprSidecarImage = "ghcr.io/dapr/daprd:edge";

        public static class EnvironmentKeys
        {
            public const string TEST_APP_ENVIRONMENT = "DAPR_E2E_TEST_APP_ENVIRONMENT";

            public const string TEST_APP_REGISTRY = "DAPR_E2E_TEST_APP_REGISTRY";

            public const string TEST_APP_TAG = "DAPR_E2E_TEST_APP_TAG";
        }
    }
}