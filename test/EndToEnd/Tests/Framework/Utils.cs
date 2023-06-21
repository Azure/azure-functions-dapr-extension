namespace EndToEndTests.Framework
{
    using System.Net;
    using System.Net.Sockets;

    public static class Utils
    {
        /// <summary>
        /// Finds a free TCP port.
        /// </summary>
        public static int GetFreeTcpPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// Gets the value of an environment variable.
        /// Throws an exception if the environment variable is not set.
        /// </summary>
        /// <param name="key">The name of the environment variable.</param>
        /// <returns>The value of the environment variable.</returns>
        /// <exception cref="SystemException">Thrown if the environment variable is not set.</exception>
        public static string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ??
                throw new SystemException($"Environment variable {key} is not set.");
        }
    }
}