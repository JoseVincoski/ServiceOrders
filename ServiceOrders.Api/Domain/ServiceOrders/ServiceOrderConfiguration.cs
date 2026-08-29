using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Domain.Users;

namespace ServiceOrders.Api.Domain.ServiceOrders;

internal sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("ServiceOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status).HasConversion<int>().IsRequired();
        builder.Property(o => o.OpeningTime).IsRequired();
        builder.Property(o => o.ClosureTime).IsRequired(false);
        builder.Property(o => o.RequestorFailureDescription).HasMaxLength(ServiceOrder.MaxRequestorFailureDescriptionLength).IsRequired();
        builder.Property(o => o.WorkerFixDescription).HasMaxLength(ServiceOrder.MaxWorkerFixDescriptionLength).IsRequired(false);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(o => o.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.RequestorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.WorkerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}