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

            bool result = EnvironmentExtensions.IsAppService(this.nameResolverMock.Object);

            if (!string.IsNullOrEmpty(instanceId))
            {
                Assert.True(result);
            }
            
            if(instanceId == null || instanceId == string.Empty)
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

            bool result = EnvironmentExtensions.IsWindowsElasticPremium(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("ElasticPremium", false)]
        [InlineData("DifferentSku", true)]
        public void ShouldRegisterDaprExtensionWithAzureWebsiteSkuShouldReturnExpectedResult(string skuValue, bool expectedResult)
        {
            this.nameResolverMock.Setup(nr => nr.Resolve(Constants.EnvironmentKeys.AzureWebsiteSku))
                                  .Returns(skuValue);

            bool result = EnvironmentExtensions.ShouldRegisterDaprExtension(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("", "",  true)]
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

            bool result = EnvironmentExtensions.ShouldRegisterDaprExtension(this.nameResolverMock.Object);

            Assert.Equal(expectedResult, result);
        }
    }
}
