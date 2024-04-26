namespace DaprExtensionTests.UnitTests.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Services;
    using Microsoft.Azure.WebJobs.Extensions.Dapr.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class DaprHttpClientTests
    {
        private readonly ILogger logger;

        public DaprHttpClientTests()
        {
            var loggerFactory = new Mock<LoggerFactory>().Object;
            this.logger = loggerFactory.CreateLogger(LoggingUtils.CreateDaprBindingCategory());
        }

        [Fact]
        public async Task PostAsync_Success()
        {
            // Arrange
            var clientFactory = new TestHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new DaprHttpClient(clientFactory);
            var content = new StringContent("Hello World");
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await httpClient.PostAsync(this.logger, "http://localhost/api", content, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAsync_Success()
        {
            // Arrange
            var clientFactory = new TestHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new DaprHttpClient(clientFactory);
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await httpClient.GetAsync(this.logger, "http://localhost/api", cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_Success()
        {
            // Arrange
            var clientFactory = new TestHttpClientFactory(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new DaprHttpClient(clientFactory);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api");
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await httpClient.SendAsync(this.logger, httpRequestMessage, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpResponseMessage responseMessage;

        public TestHttpClientFactory(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new TestHttpMessageHandler(this.responseMessage));
        }
    }

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage responseMessage;

        public TestHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.responseMessage);
        }
    }
}