using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffPropertyConfiguration : IEntityTypeConfiguration<MffProperty>
{
    public void Configure(EntityTypeBuilder<MffProperty> builder)
    {
        builder.ToTable("MFF_PROPERTIES");
        builder.HasKey(x => x.PropertyId);
        builder.HasIndex(x => x.CityCode);
        builder.HasIndex(x => x.GovernorateCode);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.RegistrationStatusCode);
        builder.HasIndex(x => x.SellerPersonId);

        builder.Property(x => x.PropertyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.GovernorateCode).HasMaxLength(50);
        builder.Property(x => x.CityCode).HasMaxLength(50);
        builder.Property(x => x.DistrictAr).HasMaxLength(200);
        builder.Property(x => x.NeighborhoodAr).HasMaxLength(200);
        builder.Property(x => x.BuildingNo).HasMaxLength(100);
        builder.Property(x => x.FloorNo).HasMaxLength(50);
        builder.Property(x => x.UnitNo).HasMaxLength(50);
        builder.Property(x => x.AreaSqm).HasPrecision(10, 2);
        builder.Property(x => x.AddressAr).HasMaxLength(1000);
        builder.Property(x => x.RegistrationStatusCode).HasMaxLength(50);
        builder.Property(x => x.RegistryReference).HasMaxLength(300);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.AppraisedValue).HasPrecision(18, 2);
        builder.Property(x => x.MaintenanceDeposit).HasPrecision(18, 2);

        builder.HasOne<MffLkpCity>().WithMany().HasForeignKey(x => x.CityCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpGovernorate>().WithMany().HasForeignKey(x => x.GovernorateCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffHousingProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpPropertyRegStatus>().WithMany().HasForeignKey(x => x.RegistrationStatusCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffPerson>().WithMany().HasForeignKey(x => x.SellerPersonId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
