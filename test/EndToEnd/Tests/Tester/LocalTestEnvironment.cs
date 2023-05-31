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
            await StartDaprContainerAsync(daprPortOnHost, appPortOnHost, appName);

            return new TestApp("http://localhost", appPortOnHost);
        }

        public async Task StopAsync(string appName)
        {
            this.logger.LogInformation($"Stopping test app {appName}.");

            // Stop and delete the application container
            // TODO: save logs from the container before deleting it for debugging purposes
            // await dockerClient.Containers.StopContainerAsync(appName, new DockerModels.ContainerStopParameters());
            // await dockerClient.Containers.RemoveContainerAsync(appName, new DockerModels.ContainerRemoveParameters());

            // Stop and delete the Dapr sidecar container
            // await dockerClient.Containers.StopContainerAsync(this.GetDaprdContainerName(appName), new DockerModels.ContainerStopParameters());
            // await dockerClient.Containers.RemoveContainerAsync(this.GetDaprdContainerName(appName), new DockerModels.ContainerRemoveParameters());
        }

        /// <summary>
        /// Starts the application in a container.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <param name="appPortOnHost">The port where the application should listen on the host.</param>
        /// <param name="daprPortOnHost">The port where the Dapr sidecar should listen on the host.</param>
        private async Task<string> StartAppContainerAsync(string appName, int appPortOnHost, int daprPortOnHost)
        {
            var containerParams = new DockerModels.CreateContainerParameters
            {
                Image = $"{containerRegistry}/{appName}:{containerTag}",
                Name = appName,
                ExposedPorts = new Dictionary<string, DockerModels.EmptyStruct>
                {
                    { $"{appPortOnHost}", new DockerModels.EmptyStruct() }
                },
                HostConfig = new DockerModels.HostConfig
                {
                    NetworkMode = "host", // App needs to be on the same network as host to talk to Dapr sidecar
                },
                Env = new List<string>
                {
                    $"DAPR_HTTP_PORT={daprPortOnHost}",
                    $"ASPNETCORE_URLS=http://localhost:{appPortOnHost}",
                }
            };

            return await CreateAndStartContainerAsync(containerParams);
        }

        /// <summary>
        /// Starts the Dapr sidecar in a container.
        /// </summary>
        /// <param name="daprPortOnHost">The port where the Dapr sidecar should listen on the host.</param>
        /// <param name="appPortOnHost">The port where the application listens on the host.</param>
        /// <param name="appName">The name of the application.</param>
        private async Task<string> StartDaprContainerAsync(int daprPortOnHost, int appPortOnHost, string appName)
        {
            var containerParams = new DockerModels.CreateContainerParameters
            {
                Image = Constants.DaprSidecarImage,
                Name = this.GetDaprdContainerName(appName),
                ExposedPorts = new Dictionary<string, DockerModels.EmptyStruct>
                {
                    { $"{daprPortOnHost}", new DockerModels.EmptyStruct() }
                },
                HostConfig = new DockerModels.HostConfig
                {
                    NetworkMode = "host", // Dapr sidecar needs to be on the same network as host to talk to components
                    Mounts = new List<DockerModels.Mount>
                    {
                        new DockerModels.Mount
                        {
                            Source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dapr", "components"),
                            Target = "/components",
                            Type = "bind"
                        },
                    },
                },
                Cmd = new List<string>
                {
                    "./daprd",
                    "--app-id", appName,
                    // "--app-port", $"{appPortOnHost}", // TODO: enable when using triggers
                    "--dapr-http-port", $"{daprPortOnHost}",
                    "--resources-path", "./components"
                }
            };
            return await CreateAndStartContainerAsync(containerParams);
        }

        private string StartDaprProcess(string appName, int appPort)
        {
            var process = new Process();
            process.StartInfo.FileName = "daprd";
            process.StartInfo.Arguments = $"--app-id {appName} --app-port {appPort} --dapr-http-port {appPort} --log-level debug";
            process.StartInfo.UseShellExecute = false;

            process.Start();

            return process.Id.ToString();
        }

        private async Task<string> CreateAndStartContainerAsync(DockerModels.CreateContainerParameters containerParams)
        {
            // TODO: Check if image exists, if not pull it
            await dockerClient.Images.CreateImageAsync(
                new DockerModels.ImagesCreateParameters
                {
                    FromImage = containerParams.Image
                },
                null,
                new Progress<DockerModels.JSONMessage>((m) => logger.LogInformation($"{m.Status}: {m.Progress}, {m.ProgressMessage}"))
            );

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

        private string GetDaprdContainerName(string appName)
        {
            return $"daprd-{appName}";
        }
    }
}