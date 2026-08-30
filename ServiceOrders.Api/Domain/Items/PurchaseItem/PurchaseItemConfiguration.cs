using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceOrders.Api.Domain.Items.PurchaseItem;

internal sealed class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("PurchaseItems");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.UnitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(p => p.PurchasedAtUtc).IsRequired();

        builder.HasOne(p => p.Item)
            .WithMany()
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}