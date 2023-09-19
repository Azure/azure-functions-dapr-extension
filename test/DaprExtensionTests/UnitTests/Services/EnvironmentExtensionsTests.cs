namespace DaprExtensionTests.UnitTests.Services
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Moq;
    using Xunit;

    public class EnvironmentExtensionsTests
    {
        private readonly Mock<INameResolver> nameResolverMock;

        public EnvironmentExtensionsTests()
        {
            this.nameResolverMock = new Mock<INameResolver>();
        }

        [Theory]
        [InlineData("AzureWebsiteInstanceIdValue")]
        [InlineData(null)]
        [InlineData("")]
        public void IsAppServiceShouldReturnExpectedResult(string instanceId)
        {
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(instanceId);

            bool result = EnvironmentUtils.IsAppService(this.nameResolverMock.Object);

            if (!string.IsNullOrEmpty(instanceId))
            {
                Assert.True(result);
            }

            if (instanceId == null || instanceId == string.Empty)
            {
                Assert.False(result);
            }
        }

        [Theory]
        [InlineData("ElasticPremium", true)]
        [InlineData("DifferentSku", false)]
        public void IsWindowsElasticPremiumShouldReturnExpectedResult(string skuValue, bool expectedResult)
        {
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku))
                                  .Returns(skuValue);

            bool result = EnvironmentUtils.IsWindowsElasticPremium(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("ElasticPremium", false)]
        [InlineData("DifferentSku", true)]
        public void ShouldRegisterDaprExtensionWithAzureWebsiteSkuShouldReturnExpectedResult(string skuValue, bool expectedResult)
        {
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku))
                                  .Returns(skuValue);

            bool result = EnvironmentUtils.ShouldRegisterDaprExtension(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData(null, null, true)]
        [InlineData("", "DifferentSku", true)]
        [InlineData("somevalue", "ElasticPremium", false)]
        [InlineData("somevalue", "DifferentSku", false)]
        [InlineData("somevalue", "", false)]
        public void ShouldRegisterDaprExtensionWithAzureWebsiteSkuAndAzureWebsiteInstanceIdShouldReturnExpectedResult(string azureWebsiteInstanceIdValue, string azureWebsiteSkuValue, bool expectedResult)
        {
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceIdValue);
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku))
                                  .Returns(azureWebsiteSkuValue);

            bool result = EnvironmentUtils.ShouldRegisterDaprExtension(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("someContainerName", "someWebsitePodName", "someLegionServiceHost", "", true)]
        [InlineData("someContainerName", "", "someLegionServiceHost", "", true)]
        [InlineData("", "someWebsitePodName", "someLegionServiceHost", "", true)]
        [InlineData("someContainerName", "someWebsitePodName", "", "", false)]
        [InlineData("", "", "someLegionServiceHost", "", false)]
        [InlineData("someContainerName", "", "", "", false)]
        [InlineData("", "someWebsitePodName", "", "", false)]
        [InlineData("", "", "", "", false)]
        public void IsLinuxConsumptionOnLegionTest(string containerName, string websitePodName, string legionServiceHost, string azureWebsiteInstanceId,  bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ContainerName)).Returns(containerName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.WebsitePodName)).Returns(websitePodName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.LegionServiceHost)).Returns(legionServiceHost);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceId);

            bool result = EnvironmentUtils.IsLinuxConsumptionOnLegion(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("someContainerName", "someWebsitePodName", "someLegionServiceHost", "", "", true)]
        [InlineData("someContainerName", "", "someLegionServiceHost", "", "", true)]
        [InlineData("", "someWebsitePodName", "someLegionServiceHost", "", "", true)]
        [InlineData("someContainerName", "someWebsitePodName", "", "", "", false)]
        [InlineData("", "", "someLegionServiceHost", "", "", false)]
        [InlineData("someContainerName", "", "", "", "", false)]
        [InlineData("", "someWebsitePodName", "", "", "", false)]
        [InlineData("", "", "", "", "", false)]
        [InlineData("", "", "", "", "FlexConsumption", true)]
        public void IsFlexConsumptionSkuTest(string containerName, string websitePodName, string legionServiceHost, string azureWebsiteInstanceId, string azureWebsiteSku, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ContainerName)).Returns(containerName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.WebsitePodName)).Returns(websitePodName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.LegionServiceHost)).Returns(legionServiceHost);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceId);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku)).Returns(azureWebsiteSku);

            bool result = EnvironmentUtils.IsFlexConsumptionSku(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("someContainerName", "", "AzureWebsiteInstanceId", false)]
        [InlineData("someContainerName", "", "", true)]
        [InlineData("someContainerName", "someLegionServiceHost", "AzureWebsiteInstanceId", false)]
        public void IsLinuxConsumptionOnAtlasTest(string containerName, string legionServiceHost, string azureWebsiteInstanceId, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ContainerName)).Returns(containerName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.LegionServiceHost)).Returns(legionServiceHost);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceId);

            bool result = EnvironmentUtils.IsLinuxConsumptionOnAtlas(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("managedEnv", true)]
        [InlineData("", false)]
        public void IsManagedAppEnvironmentTest(string managedEnv, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ManagedEnvironment)).Returns(managedEnv);

            bool result = EnvironmentUtils.IsManagedAppEnvironment(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("someContainerName", "", "", "FlexConsumption", "", true)]
        [InlineData("someContainerName", "", "", "FlexConsumption", "managedEnv", false)]
        public void IsAnyLinuxConsumptionTest(string containerName, string legionServiceHost, string azureWebsiteInstanceId, string azureWebsiteSku, string managedEnv, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ContainerName)).Returns(containerName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.LegionServiceHost)).Returns(legionServiceHost);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceId);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku)).Returns(azureWebsiteSku);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ManagedEnvironment)).Returns(managedEnv);

            bool result = EnvironmentUtils.IsAnyLinuxConsumption(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("Dynamic", true)]
        [InlineData("DynamicTest", false)]
        public void IsWindowsConsumptionTest(string azureWebsiteSku, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku)).Returns(azureWebsiteSku);

            bool result = EnvironmentUtils.IsWindowsConsumption(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData("someContainerName", "", "", "FlexConsumption", "", "someWebsitePodName", true)]
        [InlineData("", "", "", "FlexConsumption", "", "", true)]
        public void IsConsumptionSkuTest(string containerName, string legionServiceHost, string azureWebsiteInstanceId, string azureWebsiteSku, string managedEnv, string websitePodName, bool expectedValue)
        {
            var nameResolverMock = new Mock<INameResolver>();
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku)).Returns(azureWebsiteSku);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ContainerName)).Returns(containerName);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.LegionServiceHost)).Returns(legionServiceHost);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.AzureWebsiteInstanceId))
                                  .Returns(azureWebsiteInstanceId);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.ManagedEnvironment)).Returns(managedEnv);
            nameResolverMock.Setup(r => r.Resolve(Constants.EnvironmentKeys.WebsitePodName)).Returns(websitePodName);



            bool result = EnvironmentUtils.IsConsumptionSku(nameResolverMock.Object);

            Assert.Equal(expectedValue, result);
        }
    }
}
