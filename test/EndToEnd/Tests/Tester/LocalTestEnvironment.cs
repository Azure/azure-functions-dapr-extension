namespace EndToEndTests.Tester
{
    using Docker.DotNet;
    using DockerModels = Docker.DotNet.Models;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    class LocalTestEnvironment : ITestEnvironment
    {
        private ILogger logger;
        private string containerRegistry;
        private string containerTag;
        private DockerClient dockerClient;
        private int daprPid;

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

        public async Task<TestApp> StartAsync(string appName)
        {
            int appPortOnHost = Utils.FreeTcpPort();
            int daprPortOnHost = Utils.FreeTcpPort();

            // Start the application
            this.logger.LogInformation($"Starting test app {appName} on port {appPortOnHost}.");
            await StartAppContainerAsync(appName, appPortOnHost, daprPortOnHost);

            // Start the Dapr sidecar
            this.logger.LogInformation($"Starting Dapr sidecar for test app {appName} on port {daprPortOnHost}.");
            daprPid = StartDaprProcess(daprPortOnHost, appName, null);

            // Wait for the application to be ready
            // Figure out a better way to do this!
            this.logger.LogInformation($"Waiting for test app {appName} to be ready.");
            await Task.Delay(5000);

            return new TestApp("http://localhost", appPortOnHost);
        }

        public async Task StopAsync(string appName)
        {
            this.logger.LogInformation($"Stopping test app {appName}.");

            // Stop and delete the application container
            // TODO: save logs from the container before deleting it for debugging purposes
            await dockerClient.Containers.StopContainerAsync(appName, new DockerModels.ContainerStopParameters());
            await dockerClient.Containers.RemoveContainerAsync(appName, new DockerModels.ContainerRemoveParameters());

            // Stop the Dapr process
            if (daprPid != 0)
            {
                this.logger.LogInformation($"Stopping Dapr sidecar for test app {appName}.");
                Process.GetProcessById(daprPid).Kill();
            }
        }

        /// <summary>
        /// Starts the application in a container.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <param name="appPortOnHost">The port where the application should listen on the host.</param>
        /// <param name="daprPortOnHost">The port where the Dapr sidecar should listen on the host.</param>
        private async Task<string> StartAppContainerAsync(string appName, int appPortOnHost, int daprPortOnHost)
        {
            var imageName = $"{containerRegistry}/{appName}:{containerTag}";

            // Pull the image
            await dockerClient.Images.CreateImageAsync(
                new DockerModels.ImagesCreateParameters
                {
                    FromImage = imageName
                },
                null,
                new Progress<DockerModels.JSONMessage>((m) => logger.LogInformation($"{m.Status}: {m.Progress}, {m.ProgressMessage}"))
            );

            var containerParams = new DockerModels.CreateContainerParameters
            {
                Image = imageName,
                Name = appName,
                ExposedPorts = new Dictionary<string, DockerModels.EmptyStruct>
                {
                    { $"{appPortOnHost}", new DockerModels.EmptyStruct() }
                },
                HostConfig = new DockerModels.HostConfig
                {
                    PortBindings = new Dictionary<string, IList<DockerModels.PortBinding>>
                    {
                        { $"{appPortOnHost}", new List<DockerModels.PortBinding> { new DockerModels.PortBinding { HostPort = $"{appPortOnHost}" } } }
                    }
                },
                Env = new List<string>
                {
                    $"DAPR_HTTP_HOST=host.docker.internal",
                    $"DAPR_HTTP_PORT={daprPortOnHost}",
                    $"ASPNETCORE_URLS=http://*:{appPortOnHost}",
                }
            };

            // Create the container
            var response = await dockerClient.Containers.CreateContainerAsync(containerParams);

            if (response.Warnings != null)
            {
                foreach (var warning in response.Warnings)
                {
                    logger.LogWarning(warning);
                }
            }

            // Start the container
            var started = await dockerClient.Containers.StartContainerAsync(response.ID, new DockerModels.ContainerStartParameters());
            if (!started)
            {
                throw new SystemException($"Failed to start container {response.ID}");
            }

            return response.ID;
        }

        /// <summary>
        /// Starts the Daprd process.
        /// </summary>
        /// <param name="daprHttpPort">The port where the Dapr sidecar should listen.</param>
        /// <param name="appName">The name of the application.</param>
        /// <param name="appPort">The port where the application is listening.</param>
        private int StartDaprProcess(int daprHttpPort, string appName, int? appPort)
        {
            var componentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dapr", "components");

            var args = $"--app-id {appName} --dapr-http-port {daprHttpPort} --resources-path {componentsPath}";
            if (appPort.HasValue)
            {
                args += $" --app-port {appPort.Value}";
            }

            var process = new Process();
            process.StartInfo.FileName = "daprd";
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;

            // TODO: write logs to a file for debugging purposes
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            return process.Id;
        }
    }
}