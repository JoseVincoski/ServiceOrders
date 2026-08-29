using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Users;

namespace ServiceOrders.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.MigrateAsync().Wait();

        SeedMasterUser(dbContext);
    }

    private static void SeedMasterUser(ApplicationDbContext dbContext)
    {
        if (dbContext.Set<User>().Any())
        {
            return;
        }

        string masterPasswordHash = BCrypt.Net.BCrypt.HashPassword("Ornitorrinco2000");

        var masterResult = User.Create(
            "Master Admin",
            "master@admin.com",
            masterPasswordHash,
            UserRole.Master);

        if (masterResult.IsSuccess)
        {
            dbContext.Set<User>().Add(masterResult.Value);
            dbContext.SaveChanges();
        }
    }
}
