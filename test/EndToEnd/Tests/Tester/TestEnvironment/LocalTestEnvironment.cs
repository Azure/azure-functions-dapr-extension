namespace EndToEndTests.Tester
{
    using System.Net;
    using System.Net.Sockets;
    using Docker.DotNet;

    public class LocalTestEnvironment : TestEnvironmentBase
    {
        private const string daprContainerNameSuffix = "-dapr";
        private const string daprImageUrl = "mcr.microsoft.com/daprio/daprd";
        private const string daprImageTag = "latest";

        private DockerClient dockerClient;
        private Dictionary<string, string> runningContainers;

        // TODO: add logger
        LocalTestEnvironment() : base()
        {
            dockerClient = new DockerClientConfiguration().CreateClient();
            runningContainers = new Dictionary<string, string>();
        }

        public override void Setup()
        {
            // Nothing to do here.
        }

        public async override void Start(TestApp app)
        {
            int hostPort = FreeTcpPort();
            int daprPort = FreeTcpPort();

            // Required environment variables for tests to run.
            Environment.SetEnvironmentVariable(Constants.ENVKEY_HOST_URI, "http://localhost");
            Environment.SetEnvironmentVariable(Constants.ENVKEY_APP_PORT, $"{hostPort}");
            Environment.SetEnvironmentVariable(Constants.ENVKEY_DAPR_HTTP_PORT, $"{daprPort}");

            // Start the test app.
            var appContainerId = await this.CreateHostContainer(app, hostPort);
            await dockerClient.Containers.StartContainerAsync(appContainerId, new Docker.DotNet.Models.ContainerStartParameters());

            runningContainers.Add(app.Name, appContainerId);

            // Start Dapr.
            var daprContainerId = await this.CreateDaprContainer(app, daprPort);
            await dockerClient.Containers.StartContainerAsync(daprContainerId, new Docker.DotNet.Models.ContainerStartParameters());

            runningContainers.Add($"{app.Name}{daprContainerNameSuffix}", daprContainerId);
        }

        public override void Stop(TestApp app)
        {
            foreach (var containerName in runningContainers.Keys)
            {
                Console.WriteLine($"Stopping container {containerName}...");
                dockerClient.Containers.StopContainerAsync(containerName, new Docker.DotNet.Models.ContainerStopParameters());
            }
        }

        public override void TearDown()
        {
            // Nothing to do here.
        }

        private async Task<string> CreateHostContainer(TestApp app, int hostPort)
        {
            var containerParams = new Docker.DotNet.Models.CreateContainerParameters
            {
                Image = $"{base.TestAppRegistry}/{app.Name}:{base.TestAppTag}",
                Name = app.Name,
                ExposedPorts = new Dictionary<string, Docker.DotNet.Models.EmptyStruct>
                {
                    { $"{app.Port}", new Docker.DotNet.Models.EmptyStruct() }
                },
                HostConfig = new Docker.DotNet.Models.HostConfig
                {
                    PortBindings = new Dictionary<string, IList<Docker.DotNet.Models.PortBinding>>
                    {
                        { $"{app.Port}", new List<Docker.DotNet.Models.PortBinding>
                            {
                                new Docker.DotNet.Models.PortBinding
                                {
                                    HostPort = $"{hostPort}"
                                }
                            }
                        }
                    }
                }
            };

            var response = await dockerClient.Containers.CreateContainerAsync(containerParams);
            if (response.Warnings.Count > 0)
            {
                foreach (var warning in response.Warnings)
                {
                    Console.WriteLine($"WARNING: {warning}");
                }
            }

            return response.ID;
        }

        private async Task<string> CreateDaprContainer(TestApp app, int daprPort)
        {
            var containerParams = new Docker.DotNet.Models.CreateContainerParameters
            {
                Image = $"{daprImageUrl}:{daprImageTag}",
                Name = $"{app.Name}{daprContainerNameSuffix}",
                ExposedPorts = new Dictionary<string, Docker.DotNet.Models.EmptyStruct>
                {
                    { $"{daprPort}", new Docker.DotNet.Models.EmptyStruct() }
                },
                HostConfig = new Docker.DotNet.Models.HostConfig
                {
                    PortBindings = new Dictionary<string, IList<Docker.DotNet.Models.PortBinding>>
                    {
                        { $"{app.Port}", new List<Docker.DotNet.Models.PortBinding>
                            {
                                new Docker.DotNet.Models.PortBinding
                                {
                                    HostPort = $"{daprPort}"
                                }
                            }
                        }
                    }
                }
                // TODO: add components volume mount, override command with app port and dapr port
            };

            var response = await dockerClient.Containers.CreateContainerAsync(containerParams);
            if (response.Warnings.Count > 0)
            {
                foreach (var warning in response.Warnings)
                {
                    Console.WriteLine($"WARNING: {warning}");
                }
            }

            return response.ID;
        }

        /// <summary>
        /// Finds a free TCP port.
        /// </summary>
        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}