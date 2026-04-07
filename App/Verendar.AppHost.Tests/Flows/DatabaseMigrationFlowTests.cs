using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Verendar.DatabaseMigrationHelpers;
using Xunit;

namespace Verendar.AppHost.Tests.Flows;

public class DatabaseMigrationFlowTests
{
    [Fact]
    public async Task MigrateDbContextAsync_WhenMigrationFails_ShouldThrow()
    {
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<NonRelationalDbContext>(options =>
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            })
            .Build();

        var act = async () => await host.MigrateDbContextAsync<NonRelationalDbContext>();

        await act.Should().ThrowAsync<Exception>();
    }

    private sealed class NonRelationalDbContext(DbContextOptions<NonRelationalDbContext> options)
        : DbContext(options);
}
