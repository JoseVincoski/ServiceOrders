using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Domain.Sectors;

namespace ServiceOrder.Api.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<Sector> Sectors { get; set; }
}
