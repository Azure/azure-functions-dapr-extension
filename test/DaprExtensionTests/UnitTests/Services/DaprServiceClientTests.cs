
namespace DaprExtensionTests.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core;
    using Microsoft.Azure.Functions.Extensions.Dapr.Core.Utils;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class DaprServiceClientTests
    {
        private readonly Mock<ILoggerFactory> loggerFactoryMock;
        private readonly Mock<INameResolver> nameResolverMock;
        private readonly Mock<IDaprClient> daprClientMock;
        private readonly DaprServiceClient daprServiceClient;

        public DaprServiceClientTests()
        {
            this.loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

            this.nameResolverMock = new Mock<INameResolver>();
            this.daprClientMock = new Mock<IDaprClient>();

            this.daprServiceClient = new DaprServiceClient(
                this.loggerFactoryMock.Object,
                this.daprClientMock.Object,
                this.nameResolverMock.Object);
        }

        [Fact]
        public async Task SaveStateAsync_Success()
        {
            // Arrange
            var daprAddress = "http://localhost:3500";
            var stateStore = "mystore";
            var values = new List<DaprStateRecord>()
            {
                new DaprStateRecord("key1", "value1"),
                new DaprStateRecord("key2", "value2"),
            };
            var expectedUri = $"{daprAddress}/v1.0/state/{Uri.EscapeDataString(stateStore)}";
            var expectedContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(values, JsonUtils.DefaultSerializerOptions),
                System.Text.Encoding.UTF8,
                "application/json");
            this.daprClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await this.daprServiceClient.SaveStateAsync(daprAddress, stateStore, values, CancellationToken.None);

            // Assert
            this.daprClientMock.Verify(client => client.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveStateAsync_NullStateStore_ThrowsArgumentNullException()
        {
            // Arrange
            string? daprAddress = null;
            string? stateStore = null;
            var values = new List<DaprStateRecord>()
            {
                new DaprStateRecord("key1", "value1"),
                new DaprStateRecord("key2", "value2"),
            };

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.daprServiceClient.SaveStateAsync(daprAddress, stateStore, values, CancellationToken.None));
        }

        [Fact]
        public async Task GetStateAsync_Should_Return_DaprStateRecord_When_Successful()
        {
            // Arrange
            var expectedContentStream = new MemoryStream();
            var expectedETag = "\"12345\"";
            var expectedKey = "key";
            var daprAddress = "http://localhost:3500";
            var stateStore = "mystatestore";
            var key = "key";
            var cancellationToken = CancellationToken.None;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(expectedContentStream),
            };
            response.Headers.ETag = new EntityTagHeaderValue(expectedETag);

            this.nameResolverMock.Setup(r => r.Resolve("DAPR_HTTP_PORT")).Returns("3500");
            this.daprClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            var result = await daprServiceClient.GetStateAsync(daprAddress, stateStore, key, cancellationToken);
            var data = result.Value.ToString();

            // Assert
            Assert.Equal(expectedKey, result.Key);
            Assert.Equal(string.Empty, data);
            Assert.Equal(expectedETag, result.ETag);
        }

        [Fact]
        public async Task GetStateAsync_Should_Rethrow_Exception_When_Exception_Occurs()
        {
            // Arrange
            var daprAddress = "http://localhost:3500";
            var stateStore = "mystatestore";
            var key = "key";
            var cancellationToken = CancellationToken.None;
            var errorCode = "ERR_TEST";

            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.Content = new StringContent(
                JsonSerializer.Serialize(new { message = "An error occurred", errorCode = errorCode }));

            this.nameResolverMock.Setup(r => r.Resolve("DAPR_HTTP_PORT")).Returns("3500");
            this.daprClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                               .ThrowsAsync(new DaprException(HttpStatusCode.InternalServerError, errorCode, "An error occurred"));

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<DaprException>(() =>
                daprServiceClient.GetStateAsync(daprAddress, stateStore, key, cancellationToken));

            // Assert
            Assert.Equal("An error occurred", ex.Message);
        }

        [Fact]
        public async Task InvokeMethodAsync_SendsCorrectHttpRequestMessage()
        {
            // Arrange
            string expectedAppId = "test-app-id";
            string expectedMethodName = "test-method-name";
            string expectedHttpVerb = "POST";
            object expectedBody = new { test = "value" };
            string expectedDaprAddress = "http://test-dapr-address";
            CancellationToken cancellationToken = CancellationToken.None;

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.InvokeMethodAsync(expectedDaprAddress, expectedAppId, expectedMethodName, expectedHttpVerb, expectedBody, cancellationToken);

            // Assert
            this.daprClientMock.Verify(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task SendToDaprBindingAsync_SendsCorrectHttpRequestMessage()
        {
            // Arrange
            string expectedDaprAddress = "http://test-dapr-address";
            var metadata = new Dictionary<string, object> { { "test", "metadata-value" } };

            var expectedMessage = new DaprBindingMessage(new { test = "value" }, metadata, "test-binding-name");
            CancellationToken cancellationToken = CancellationToken.None;
            this.daprClientMock
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this.daprClientMock.Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.SendToDaprBindingAsync(expectedDaprAddress, expectedMessage, cancellationToken);

            // Assert
            this.daprClientMock.Verify(client => client.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PublishEventAsync_SendsCorrectHttpRequestMessage()
        {
            // Arrange
            string expectedDaprAddress = "http://test-dapr-address";
            string expectedName = "test-name";
            string expectedTopicName = "test-topic-name";
            var expectedPayload = new { test = "value" };
            CancellationToken cancellationToken = CancellationToken.None;

            this.daprClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.PublishEventAsync(expectedDaprAddress, expectedName, expectedTopicName,
                                                       JsonDocument.Parse(JsonSerializer.Serialize(expectedPayload)).RootElement,
                                                       cancellationToken);

            // Assert
            this.daprClientMock.Verify(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task GetSecretAsync_SendsCorrectHttpRequestMessage()
        {
            // Arrange
            string expectedDaprAddress = "http://test-dapr-address";
            string expectedSecretStoreName = "test-secret-store-name";
            string expectedKey = "test-key";
            string expectedMetadata = "test-metadata";
            CancellationToken cancellationToken = CancellationToken.None;
            string expectedSecretPayload = "{\"test\":\"value\"}";

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse.Content = new StringContent(expectedSecretPayload, Encoding.UTF8, "application/json");
            this.daprClientMock.Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            var daprServiceClient = new DaprServiceClient(this.loggerFactoryMock.Object, this.daprClientMock.Object, this.nameResolverMock.Object);

            // Act
            JsonDocument actual = await daprServiceClient.GetSecretAsync(expectedDaprAddress, expectedSecretStoreName, expectedKey, expectedMetadata, cancellationToken);

            // Assert
            Assert.Equal(expectedSecretPayload, actual.RootElement.GetRawText());
            this.daprClientMock.Verify(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3501)]
        public void GetDaprHttpAddress(int? daprPort)
        {
            var nameResolverMock = new Mock<INameResolver>();
            if (daprPort.HasValue)
            {
                nameResolverMock.Setup(r => r.Resolve("DAPR_HTTP_PORT")).Returns(daprPort.Value.ToString());
            }

            var actual = DaprServiceClient.GetDaprHttpAddress(new Mock<ILogger>().Object, nameResolverMock.Object);
            var expected = daprPort.HasValue ? $"http://localhost:{daprPort.Value}" : "http://localhost:3500";

            Assert.Equal(expected, actual);
        }
    }
}

