using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlRateTypeConfiguration : IEntityTypeConfiguration<GlRateType>
{
    public void Configure(EntityTypeBuilder<GlRateType> builder)
    {
        builder.ToTable("GL_RATE_TYPES");
        builder.HasKey(x => x.RateTypeId);
        builder.HasIndex(x => x.RateTypeCode).IsUnique();

        builder.Property(x => x.RateTypeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RateTypeCode).HasMaxLength(30);
        builder.Property(x => x.RateTypeNameEn).HasMaxLength(120);
        builder.Property(x => x.RateTypeNameAr).HasMaxLength(120);
        builder.Property(x => x.RateSourceCode).HasMaxLength(30);
        builder.Property(x => x.RatePurpose).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
