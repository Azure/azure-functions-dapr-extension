namespace EndToEndTests.Tester
{
    public class TestApp
    {
        private const string EndToEndTestAppsPath = "test/EndToEnd/Apps";

        public string Name { get; private set; }

        public string Path => $"{EndToEndTestAppsPath}/{Name}";

        public int Port { get; private set; }

        public TestApp(string name, int port)
        {
            Name = name;
            Port = port;
        }
    }
}