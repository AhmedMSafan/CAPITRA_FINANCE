using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgProjectConfiguration : IEntityTypeConfiguration<MtgProject>
{
    public void Configure(EntityTypeBuilder<MtgProject> builder)
    {
        builder.ToTable("MTG_PROJECTS");
        builder.HasKey(x => x.ProjectId);
        builder.HasIndex(x => x.ProjectCode).IsUnique();
        builder.HasIndex(x => x.DeveloperId);

        builder.Property(x => x.ProjectId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ProjectCode).HasMaxLength(30);
        builder.Property(x => x.ProjectNameAr).HasMaxLength(300);
        builder.Property(x => x.ProjectNameEn).HasMaxLength(300);
        builder.Property(x => x.LocationText).HasMaxLength(1000);
        builder.Property(x => x.LandArea).HasPrecision(18, 2);
        builder.Property(x => x.TitleDeedNo).HasMaxLength(100);
        builder.Property(x => x.BuildingLicenseNo).HasMaxLength(100);
        builder.Property(x => x.ProjectCompletionPct).HasPrecision(9, 6);
        builder.Property(x => x.ProjectStatus).HasMaxLength(50);

        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
