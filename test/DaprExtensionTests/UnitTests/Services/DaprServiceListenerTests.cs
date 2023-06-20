// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprExtensionTests.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
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
            this.nameResolverMock.Setup(x => x.Resolve("DAPR_APP_PORT")).Returns("3001");
            this.nameResolverMock.Setup(x => x.Resolve("DAPR_HTTP_PORT")).Returns("3500"); // Set to suppress any logs
            this.nameResolverMock.Setup(x => x.Resolve("DAPR_ENABLE_SIDECAR_METADATA_CHECK")).Returns("true"); // Set to suppress any logs

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
            this.daprClientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));

            // Act
            await this.daprServiceListener.WarnIfSidecarMisconfigured();

            // Assert
            this.loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => (o.ToString() ?? string.Empty).StartsWith("Failed to query the Metadata API")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            this.loggerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(GetWarnIfSidecarMisconfiguredTestData))]
        public async Task WarnIfSidecarMisconfigured_Scenarios(string responseBody, LogLevel logLevel, string logMessageStartMatch)
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody),
            };
            this.daprClientMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(mockResponse));

            // Act
            await this.daprServiceListener.WarnIfSidecarMisconfigured();

            // Assert
            this.loggerMock.Verify(x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => (o.ToString() ?? string.Empty).StartsWith(logMessageStartMatch)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            this.loggerMock.VerifyNoOtherCalls();
        }

        public static IEnumerable<object[]> GetWarnIfSidecarMisconfiguredTestData()
        {
            // When Metadata API does not return a valid JSON
            yield return new object[] { "invalidJson", LogLevel.Error, "Failed to deserialize the Metadata API response" };

            // When Metadata API returns a valid JSON but does not contain appConnectionProperties
            yield return new object[] { "{\"id\": \"test-id\"}", LogLevel.Debug, "appConnectionProperties not found in metadata API" };

            // When Metadata API returns appConnectionProperties but it is not a JSON object
            yield return new object[] { "{\"appConnectionProperties\": \"test\"}", LogLevel.Error, "Failed to parse appConnectionProperties" };

            // When Metadata API return appConnectionProperties but it does not have port
            yield return new object[] { "{\"appConnectionProperties\": { \"channelAddress\": \"127.0.0.1\" } }", LogLevel.Error,
                "Failed to parse appConnectionProperties" };

            // When Metadata API return appConnectionProperties but it does not have channelAddress
            yield return new object[] { "{\"appConnectionProperties\": { \"port\": 3001 } }", LogLevel.Error,
                "Failed to parse appConnectionProperties" };

            // When Metadata API return appConnectionProperties with invalid port
            yield return new object[] { "{\"appConnectionProperties\": { \"port\": 3002, \"channelAddress\": \"127.0.0.1\" } }", LogLevel.Warning,
                "The Dapr sidecar is configured to listen on port 3002, but the app server is running on port 3001." };

            // When Metadata API return appConnectionProperties with invalid channelAddress
            yield return new object[] { "{\"appConnectionProperties\": { \"port\": 3001, \"channelAddress\": \"127.0.0.2\" } }", LogLevel.Warning,
                "The Dapr sidecar is configured to listen on host 127.0.0.2, but the app server is running on host 127.0.0.1." };
        }
    }
}