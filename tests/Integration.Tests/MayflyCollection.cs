namespace Integration.Tests;

[CollectionDefinition(Name)]
public sealed class MayflyCollection : ICollectionFixture<MayflyFactory>
{
    public const string Name = "Mayfly";
}
