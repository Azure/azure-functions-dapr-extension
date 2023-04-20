
namespace DaprExtensionTests.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Moq;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr;
    using Microsoft.Azure.WebJobs;
    using System.Timers;
    using System;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Moq.Protected;
    using System.Data;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Exceptions;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    public class DaprServiceClientTests
    {
        private readonly Mock<IHttpClientFactory> httpClientFactoryMock;
        private readonly Mock<INameResolver> nameResolverMock;
        private readonly Mock<HttpMessageHandler> httpMessageHandlerMock;
        private readonly HttpClient httpClient;
        private readonly DaprServiceClient daprServiceClient;

        public DaprServiceClientTests()
        {
            this.httpClientFactoryMock = new Mock<IHttpClientFactory>();
            this.nameResolverMock = new Mock<INameResolver>();
            this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(this.httpMessageHandlerMock.Object);
            this.httpClientFactoryMock
                .Setup(factory => factory.CreateClient("DaprServiceClient"))
                .Returns(this.httpClient);
            this.daprServiceClient = new DaprServiceClient(
                this.httpClientFactoryMock.Object,
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
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString() == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Act
            await this.daprServiceClient.SaveStateAsync(daprAddress, stateStore, values, CancellationToken.None);

            // Assert
            this.httpMessageHandlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString() == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                );
            var actualContent = await expectedResponse.Content.ReadAsStringAsync();
            Assert.Equal(string.Empty, actualContent);
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

            var httpClient = new HttpClient(new MockHttpMessageHandler(response));
            this.httpClientFactoryMock.Setup(f => f.CreateClient("DaprServiceClient")).Returns(httpClient);

            this.nameResolverMock.Setup(r => r.Resolve("DAPR_HTTP_PORT")).Returns("3500");

            var sut = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            var result = await sut.GetStateAsync(daprAddress, stateStore, key, cancellationToken);
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

            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.Content = new StringContent(
                JsonSerializer.Serialize(new { message = "An error occurred", errorCode = "ERR_TEST" }));

            var httpClient = new HttpClient(new MockHttpMessageHandler(response));
            this.httpClientFactoryMock.Setup(f => f.CreateClient("DaprServiceClient")).Returns(httpClient);

            this.nameResolverMock.Setup(r => r.Resolve("DAPR_HTTP_PORT")).Returns("3500");

            var sut = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            var ex = await Assert.ThrowsAsync<DaprException>(() =>
                sut.GetStateAsync(daprAddress, stateStore, key, cancellationToken));

            // Assert
            Assert.Equal("Status Code: InternalServerError; Error Code: \"ERR_TEST\" ; Message: \"An error occurred\"", ex.ToString());
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

            HttpRequestMessage actualRequest = null!;
            Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerFunc = request =>
            {
                actualRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            _ = this.httpClientFactoryMock
                .Setup(cf => cf.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new TestHandler(_handlerFunc)));

            var daprServiceClient = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.InvokeMethodAsync(expectedDaprAddress, expectedAppId, expectedMethodName, expectedHttpVerb, expectedBody, cancellationToken);

            // Assert
            Assert.NotNull(actualRequest);
            Assert.Equal(new HttpMethod(expectedHttpVerb), actualRequest.Method);
            Assert.Equal($"{expectedDaprAddress}/v1.0/invoke/{expectedAppId}/method/{expectedMethodName}", actualRequest.RequestUri!.ToString());
            Assert.Equal("application/json", actualRequest.Content!.Headers.ContentType!.MediaType);

            string actualContent = await actualRequest.Content.ReadAsStringAsync();
            Assert.Equal("{\"test\":\"value\"}", actualContent);
        }

        [Fact]
        public async Task SendToDaprBindingAsync_SendsCorrectHttpRequestMessage()
        {
            // Arrange
            string expectedDaprAddress = "http://test-dapr-address";
            var metadata = new Dictionary<string, object> { { "test", "metadata-value" } };

            var expectedMessage = new DaprBindingMessage(new { test = "value" }, metadata, "test-binding-name");
            CancellationToken cancellationToken = CancellationToken.None;

            HttpRequestMessage actualRequest = null!;
            Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerFunc = request =>
            {
                actualRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };
            this.httpClientFactoryMock
                .Setup(cf => cf.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new TestHandler(_handlerFunc)));

            var daprServiceClient = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.SendToDaprBindingAsync(expectedDaprAddress, expectedMessage, cancellationToken);

            // Assert
            Assert.NotNull(actualRequest);
            Assert.Equal(HttpMethod.Post, actualRequest.Method);
            Assert.Equal($"{expectedDaprAddress}/v1.0/bindings/{expectedMessage.BindingName}", actualRequest.RequestUri!.ToString());
            Assert.Equal("application/json", actualRequest.Content!.Headers.ContentType!.MediaType);

            string actualContent = await actualRequest.Content.ReadAsStringAsync();
            Assert.Equal("{\"data\":{\"test\":\"value\"},\"metadata\":{\"test\":\"metadata-value\"}}", actualContent);
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

            HttpRequestMessage actualRequest = null!;
            Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerFunc = request =>
            {
                actualRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };
            this.httpClientFactoryMock
                .Setup(cf => cf.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new TestHandler(_handlerFunc)));

            var daprServiceClient = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            await daprServiceClient.PublishEventAsync(expectedDaprAddress, expectedName, expectedTopicName,
                                                       JsonDocument.Parse(JsonSerializer.Serialize(expectedPayload)).RootElement,
                                                       cancellationToken);

            // Assert
            Assert.NotNull(actualRequest);
            Assert.Equal(HttpMethod.Post, actualRequest.Method);
            Assert.Equal($"{expectedDaprAddress}/v1.0/publish/{expectedName}/{expectedTopicName}", actualRequest.RequestUri!.ToString());
            Assert.Equal("application/json", actualRequest.Content!.Headers.ContentType!.MediaType);

            string actualContent = await actualRequest.Content.ReadAsStringAsync();
            Assert.Equal("{\"test\":\"value\"}", actualContent);
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

            HttpRequestMessage actualRequest = null!;
            Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerFunc = request =>
            {
                actualRequest = request;
                var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponse.Content = new StringContent(expectedSecretPayload, Encoding.UTF8, "application/json");
                return Task.FromResult(httpResponse);
            };
            this.httpClientFactoryMock
                .Setup(cf => cf.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new TestHandler(_handlerFunc)));

            var daprServiceClient = new DaprServiceClient(this.httpClientFactoryMock.Object, this.nameResolverMock.Object);

            // Act
            JsonDocument actual = await daprServiceClient.GetSecretAsync(expectedDaprAddress, expectedSecretStoreName, expectedKey, expectedMetadata, cancellationToken);

            // Assert
            Assert.NotNull(actualRequest);
            Assert.Equal(HttpMethod.Get, actualRequest.Method);
            Assert.Equal($"{expectedDaprAddress}/v1.0/secrets/{expectedSecretStoreName}/{expectedKey}?{expectedMetadata}", actualRequest.RequestUri!.ToString());
            Assert.Equal(expectedSecretPayload, actual.RootElement.GetRawText());
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage response;

            public MockHttpMessageHandler(HttpResponseMessage response)
            {
                this.response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(this.response);
                return tcs.Task;
            }
        }

        public class TestHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerFunc;

            public TestHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handlerFunc)
            {
                this._handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (this._handlerFunc == null)
                {
                    return Task.FromResult<HttpResponseMessage>(null!);
                }

                return this._handlerFunc(request);
            }
        }
    }
}

