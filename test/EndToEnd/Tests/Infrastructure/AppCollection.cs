namespace EndToEndTests.Infrastructure
{
    using Xunit;

    [CollectionDefinition("AppCollection")]
    public class AppCollection : ICollectionFixture<AppFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}