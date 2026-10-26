using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffLkpCityConfiguration : IEntityTypeConfiguration<MffLkpCity>
{
    public void Configure(EntityTypeBuilder<MffLkpCity> builder)
    {
        builder.ToTable("MFF_LKP_CITYS");
        builder.HasKey(x => x.Code);
        builder.HasIndex(x => x.GovernorateCode);

        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.GovernorateCode).HasMaxLength(50);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);

        builder.HasOne<MffLkpGovernorate>().WithMany().HasForeignKey(x => x.GovernorateCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
