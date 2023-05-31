namespace EndToEndTests.Tester
{
    using Docker.DotNet;
    using DockerModels = Docker.DotNet.Models;
    using Microsoft.Extensions.Logging;

    class LocalTestEnvironment : ITestEnvironment
    {
        private ILogger logger;
        private string containerRegistry;
        private string containerTag;
        private DockerClient dockerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalTestEnvironment"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="containerRegistry">The container registry where the test app is located.</param>
        /// <param name="containerTag">The tag to use for the test app image.</param>
        public LocalTestEnvironment(ILogger logger, string containerRegistry, string containerTag)
        {
            this.logger = logger;
            this.containerRegistry = containerRegistry;
            this.containerTag = containerTag;
            this.dockerClient = new DockerClientConfiguration().CreateClient();
        }

        public async Task<TestApp> StartAsync(string appName, int appPort)
        {
            // Start the application
            int appPortOnHost = Utils.FreeTcpPort();
            await StartAppContainerAsync(appName, appPort, appPortOnHost);

            // Start the Dapr sidecar
            int daprPort = Constants.DefaultDaprHttpPort;
            int daprPortOnHost = Utils.FreeTcpPort();
            await StartDaprContainerAsync(daprPort, daprPortOnHost, appPortOnHost);

            return new TestApp("http://localhost", appPort);
        }

        public Task StopAsync(string appName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the application in a container.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <param name="appPort">The port where the application listens on inside the container.</param>
        /// <param name="appPortOnHost">The port where the application should listen on the host.</param>
        private async Task<string> StartAppContainerAsync(string appName, int appPort, int appPortOnHost)
        {
            var containerParams = new DockerModels.CreateContainerParameters
            {
                Image = $"{containerRegistry}/{appName}:{containerTag}",
                Name = appName,
                ExposedPorts = new Dictionary<string, DockerModels.EmptyStruct>
                {
                    { $"{appPort}/tcp", new DockerModels.EmptyStruct() }
                },
                HostConfig = new DockerModels.HostConfig
                {
                    PortBindings = new Dictionary<string, IList<DockerModels.PortBinding>>
                    {
                        { $"{appPort}/tcp", new List<DockerModels.PortBinding> { new DockerModels.PortBinding { HostPort = $"{appPortOnHost}" } } }
                    }
                }
            };
            return await CreateAndStartContainerAsync(containerParams);
        }

        /// <summary>
        /// Starts the Dapr sidecar in a container.
        /// </summary>
        private async Task<string> StartDaprContainerAsync(int daprPort, int daprPortOnHost, int appPortOnHost)
        {
            // TODO
            var containerParams = new DockerModels.CreateContainerParameters { };
            return await CreateAndStartContainerAsync(containerParams);
        }

        private async Task<string> CreateAndStartContainerAsync(DockerModels.CreateContainerParameters containerParams)
        {
            var response = await dockerClient.Containers.CreateContainerAsync(containerParams);

            if (response.Warnings != null)
            {
                foreach (var warning in response.Warnings)
                {
                    logger.LogWarning(warning);
                }
            }

            var started = await dockerClient.Containers.StartContainerAsync(response.ID, new DockerModels.ContainerStartParameters());
            if (!started)
            {
                throw new SystemException($"Failed to start container {response.ID}");
            }

            return response.ID;
        }
    }
}