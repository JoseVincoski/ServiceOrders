using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using ServiceOrder.Api.Database;
using System.Data.Common;

namespace ServiceOrders.IntegrationTests.Setup;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    protected readonly ApplicationDbContext DbContext;
    protected readonly HttpClient HttpClient;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        HttpClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.MigrateAsync();

        if (_respawner == null)
        {
            _dbConnection = new NpgsqlConnection(DbContext.Database.GetConnectionString());
            await _dbConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
        }

        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task DisposeAsync()
    {
        _scope.Dispose();
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
        }
    }
}
