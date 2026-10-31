using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpGuaranteeTypeConfiguration : IEntityTypeConfiguration<AdmLkpGuaranteeType>
{
    public void Configure(EntityTypeBuilder<AdmLkpGuaranteeType> builder)
    {
        builder.ToTable("ADM_LKP_GUARANTEE_TYPES");
        builder.HasKey(x => x.GuaranteeTypeCode);

        builder.Property(x => x.GuaranteeTypeCode).HasMaxLength(50);
        builder.Property(x => x.GuaranteeNameAr).HasMaxLength(300);
        builder.Property(x => x.GuaranteeNameEn).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
