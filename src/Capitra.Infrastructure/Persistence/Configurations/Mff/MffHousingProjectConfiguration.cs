using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffHousingProjectConfiguration : IEntityTypeConfiguration<MffHousingProject>
{
    public void Configure(EntityTypeBuilder<MffHousingProject> builder)
    {
        builder.ToTable("MFF_HOUSING_PROJECTS");
        builder.HasKey(x => x.ProjectId);
        builder.HasIndex(x => x.ProjectCode).IsUnique();
        builder.HasIndex(x => x.CityCode);
        builder.HasIndex(x => x.GovernorateCode);
        builder.HasIndex(x => x.AnnouncementId);

        builder.Property(x => x.ProjectId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProjectCode).HasMaxLength(50);
        builder.Property(x => x.ProjectNameAr).HasMaxLength(500);
        builder.Property(x => x.GovernorateCode).HasMaxLength(50);
        builder.Property(x => x.CityCode).HasMaxLength(50);
        builder.Property(x => x.DistrictAr).HasMaxLength(200);
        builder.Property(x => x.NeighborhoodAr).HasMaxLength(200);
        builder.Property(x => x.SiteDescription).HasMaxLength(1000);
        builder.Property(x => x.DefaultAreaSqm).HasPrecision(10, 2);

        builder.HasOne<MffLkpCity>().WithMany().HasForeignKey(x => x.CityCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffHousingAnnouncement>().WithMany().HasForeignKey(x => x.AnnouncementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpGovernorate>().WithMany().HasForeignKey(x => x.GovernorateCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
