// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests.UnitTests.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class DaprServiceListenerTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<INameResolver> nameResolverMock;
        private readonly Mock<IDaprClient> daprClientMock;
        private readonly DaprServiceListener daprServiceListener;

        public DaprServiceListenerTests()
        {
            this.loggerMock = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(this.loggerMock.Object);

            this.nameResolverMock = new Mock<INameResolver>();
            this.nameResolverMock.Setup(x => x.Resolve("DAPR_HTTP_PORT")).Returns("3500"); // Set to suppress any logs

            this.daprClientMock = new Mock<IDaprClient>();

            this.daprServiceListener = new DaprServiceListener(
                loggerFactory.Object,
                this.daprClientMock.Object,
                this.nameResolverMock.Object);
        }

        [Fact]
        public async Task WarnIfSidecarMisconfigured_WhenDaprSidecarIsNotRunning()
        {
            // Arrange
            this.nameResolverMock.Setup(x => x.Resolve("DAPR_APP_PORT")).Returns("3001");
            this.daprClientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));

            // Act
            await this.daprServiceListener.WarnIfSidecarMisconfigured();

            // Assert
            this.loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => (o.ToString() ?? string.Empty).StartsWith("Failed to query the Metadata API")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            this.loggerMock.VerifyNoOtherCalls();
        }
    }
}