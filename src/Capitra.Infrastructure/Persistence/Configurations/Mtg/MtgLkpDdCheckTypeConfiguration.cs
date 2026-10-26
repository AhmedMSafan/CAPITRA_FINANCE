using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpDdCheckTypeConfiguration : IEntityTypeConfiguration<MtgLkpDdCheckType>
{
    public void Configure(EntityTypeBuilder<MtgLkpDdCheckType> builder)
    {
        builder.ToTable("MTG_LKP_DD_CHECK_TYPES");
        builder.HasKey(x => x.CheckTypeCode);

        builder.Property(x => x.CheckTypeCode).HasMaxLength(50);
        builder.Property(x => x.CheckNameAr).HasMaxLength(300);
        builder.Property(x => x.CheckNameEn).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
