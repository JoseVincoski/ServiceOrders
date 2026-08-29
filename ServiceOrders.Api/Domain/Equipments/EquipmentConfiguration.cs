using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceOrders.Api.Domain.Equipments;

internal sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(Equipment.MaxNameLength).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(Equipment.MaxDescriptionLength).IsRequired();

        builder.HasOne(e => e.Sector)
            .WithMany()
            .HasForeignKey(e => e.SectorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}