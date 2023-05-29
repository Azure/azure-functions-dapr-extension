namespace EndToEndTests.Tester
{
    public class TestApp
    {
        public string Name { get; private set; }

        public int Port { get; private set; }

        public TestApp(string name, int port)
        {
            Name = name;
            Port = port;
        }
    }
}