using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqLkpLocationConfiguration : IEntityTypeConfiguration<ChqLkpLocation>
{
    public void Configure(EntityTypeBuilder<ChqLkpLocation> builder)
    {
        builder.ToTable("CHQ_LKP_LOCATIONS");
        builder.HasKey(x => x.LocationCode);

        builder.Property(x => x.LocationCode).HasMaxLength(50);
        builder.Property(x => x.LocationName).HasMaxLength(200);
        builder.Property(x => x.LocationType).HasMaxLength(50);

        builder.ConfigureAudit();
    }
}
