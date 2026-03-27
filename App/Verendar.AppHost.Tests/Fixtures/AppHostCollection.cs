namespace Verendar.AppHost.Tests.Fixtures;

using Xunit;

[CollectionDefinition(Name)]
public sealed class AppHostCollection : ICollectionFixture<AppHostFixture>
{
    public const string Name = "apphost-collection";
}
