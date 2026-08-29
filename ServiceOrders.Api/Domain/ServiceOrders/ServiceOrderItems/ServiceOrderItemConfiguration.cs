using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceOrders.Api.Domain.Items;

namespace ServiceOrders.Api.Domain.ServiceOrders.ServiceOrderItems;

internal sealed class ServiceOrderItemConfiguration : IEntityTypeConfiguration<ServiceOrderItem>
{
    public void Configure(EntityTypeBuilder<ServiceOrderItem> builder)
    {
        builder.ToTable("ServiceOrderItems");
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Quantity).IsRequired();

        builder.HasOne<ServiceOrder>()
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Item>()
            .WithMany()
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}