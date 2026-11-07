using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffHousingAnnouncementConfiguration : IEntityTypeConfiguration<MffHousingAnnouncement>
{
    public void Configure(EntityTypeBuilder<MffHousingAnnouncement> builder)
    {
        builder.ToTable("MFF_HOUSING_ANNOUNCEMENTS");
        builder.HasKey(x => x.AnnouncementId);
        builder.HasIndex(x => x.AnnouncementCode).IsUnique();

        builder.Property(x => x.AnnouncementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AnnouncementCode).HasMaxLength(50);
        builder.Property(x => x.AnnouncementNameAr).HasMaxLength(250);
        builder.Property(x => x.SourceReference).HasMaxLength(500);

        builder.ConfigureAudit();
    }
}
