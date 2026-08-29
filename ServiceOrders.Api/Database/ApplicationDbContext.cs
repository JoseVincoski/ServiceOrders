using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Domain.ServiceOrders;
using ServiceOrders.Api.Domain.ServiceOrders.ServiceOrderItems;
using ServiceOrders.Api.Domain.Users;

namespace ServiceOrders.Api.Database;

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
    public DbSet<Item> Items { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }
    public DbSet<ServiceOrder> ServiceOrders { get; set; }
    public DbSet<User> Users { get; set; }

}
