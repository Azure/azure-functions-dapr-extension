namespace EndToEndTests.Infrastructure
{
    using System.Net;
    using System.Net.Sockets;

    public static class Utils
    {
        /// <summary>
        /// Finds a free TCP port.
        /// </summary>
        public static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}