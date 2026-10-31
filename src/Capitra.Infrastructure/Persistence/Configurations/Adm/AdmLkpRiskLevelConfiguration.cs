using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpRiskLevelConfiguration : IEntityTypeConfiguration<AdmLkpRiskLevel>
{
    public void Configure(EntityTypeBuilder<AdmLkpRiskLevel> builder)
    {
        builder.ToTable("ADM_LKP_RISK_LEVELS");
        builder.HasKey(x => x.RiskLevelCode);

        builder.Property(x => x.RiskLevelCode).HasMaxLength(20);
        builder.Property(x => x.RiskLevelNameAr).HasMaxLength(100);
        builder.Property(x => x.RiskLevelNameEn).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
