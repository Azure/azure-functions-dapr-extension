namespace EndToEndTests.Framework
{
    public class TestApp
    {
        public string Host { get; private set; }

        public int Port { get; private set; }

        public TestApp(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}