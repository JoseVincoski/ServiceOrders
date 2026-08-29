using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceOrders.Api.Domain.Items;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name).HasMaxLength(Item.MaxNameLength).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(Item.MaxDescriptionLength).IsRequired();
    }
}