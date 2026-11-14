using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpInsuranceTypeConfiguration : IEntityTypeConfiguration<AdmLkpInsuranceType>
{
    public void Configure(EntityTypeBuilder<AdmLkpInsuranceType> builder)
    {
        builder.ToTable("ADM_LKP_INSURANCE_TYPES");
        builder.HasKey(x => x.InsuranceTypeCode);

        builder.Property(x => x.InsuranceTypeCode).HasMaxLength(30);
        builder.Property(x => x.InsuranceNameAr).HasMaxLength(200);
        builder.Property(x => x.InsuranceNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
