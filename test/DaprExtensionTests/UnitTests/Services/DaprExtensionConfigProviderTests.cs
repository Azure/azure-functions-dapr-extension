namespace DaprExtensionTests.UnitTests.Services
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class DaprExtensionConfigProviderTests
    {
        private readonly Mock<INameResolver> nameResolverMock;
        private readonly Mock<ILogger> loggerFactory;

        public DaprExtensionConfigProviderTests()
        {
            this.nameResolverMock = new Mock<INameResolver>();
            this.loggerFactory = new Mock<ILogger>();
        }

        [Fact]
        public void SetRestrictedHostingEnvironmentsToNullTest()
        {
            //Arrange
            string currentHostingEnv = "WEBSITE_INSTANCE_ID";
            string? restrictedHostingEnvironments = null;
            this.nameResolverMock.Setup(x => x.Resolve(Constants.EnvironmentKeys.RestrictedHostingEnvironments))
                .Returns(restrictedHostingEnvironments!);
            this.nameResolverMock.Setup(x => x.Resolve(currentHostingEnv))
                .Returns("1");

            //Act
            var shouldRegisterDaprExtension = DaprExtensionConfigProvider.ShouldRegisterDaprExtension(this.nameResolverMock.Object, this.loggerFactory.Object);

            //Assert
            Assert.True(shouldRegisterDaprExtension);
        }

        [Fact]
        public void SetRestrictedHostingEnvironmentsToEmptyTest()
        {
            //Arrange
            string currentHostingEnv = "WEBSITE_INSTANCE_ID";
            string restrictedHostingEnvironments = string.Empty;
            this.nameResolverMock.Setup(x => x.Resolve(Constants.EnvironmentKeys.RestrictedHostingEnvironments))
                .Returns(restrictedHostingEnvironments!);
            this.nameResolverMock.Setup(x => x.Resolve(currentHostingEnv))
                .Returns("1");

            //Act
            var shouldRegisterDaprExtension = DaprExtensionConfigProvider.ShouldRegisterDaprExtension(this.nameResolverMock.Object, this.loggerFactory.Object);

            //Assert
            Assert.True(shouldRegisterDaprExtension);
        }

        [Fact]
        public void SetRestrictedHostingEnvironmentsToAppServiceTest()
        {
            //Arrange
            string currentHostingEnv = "WEBSITE_INSTANCE_ID";
            string restrictedHostingEnvironments = currentHostingEnv;
            this.nameResolverMock.Setup(x => x.Resolve(Constants.EnvironmentKeys.RestrictedHostingEnvironments))
                .Returns(restrictedHostingEnvironments!);
            this.nameResolverMock.Setup(x => x.Resolve(currentHostingEnv))
                .Returns("1");

            //Act
            var shouldRegisterDaprExtension = DaprExtensionConfigProvider.ShouldRegisterDaprExtension(this.nameResolverMock.Object, this.loggerFactory.Object);

            //Assert
            Assert.False(shouldRegisterDaprExtension);
        }

        [Fact]
        public void SetRestrictedHostingEnvironmentsToFunctionsPremiumTest()
        {
            //Arrange
            string currentHostingEnv = "FunctionsPremium";
            string restrictedHostingEnvironments = "WEBSITE_INSTANCE_ID, FunctionsPremium";
            this.nameResolverMock.Setup(x => x.Resolve(Constants.EnvironmentKeys.RestrictedHostingEnvironments))
                .Returns(restrictedHostingEnvironments!);
            this.nameResolverMock.Setup(x => x.Resolve(currentHostingEnv))
                .Returns("1");

            //Act
            var shouldRegisterDaprExtension = DaprExtensionConfigProvider.ShouldRegisterDaprExtension(this.nameResolverMock.Object, this.loggerFactory.Object);

            //Assert
            Assert.False(shouldRegisterDaprExtension);
        }

        [Fact]
        public void SetRestrictedHostingEnvironmentsToAzureContainerAppsTest()
        {
            //Arrange
            string currentHostingEnv = "AzureContainerApps";
            string restrictedHostingEnvironments = "WEBSITE_INSTANCE_ID, FunctionsPremium";
            this.nameResolverMock.Setup(x => x.Resolve(Constants.EnvironmentKeys.RestrictedHostingEnvironments))
                .Returns(restrictedHostingEnvironments!);
            this.nameResolverMock.Setup(x => x.Resolve(currentHostingEnv))
                .Returns("1");

            //Act
            var shouldRegisterDaprExtension = DaprExtensionConfigProvider.ShouldRegisterDaprExtension(this.nameResolverMock.Object, this.loggerFactory.Object);

            //Assert
            Assert.True(shouldRegisterDaprExtension);
        }
    }
}
