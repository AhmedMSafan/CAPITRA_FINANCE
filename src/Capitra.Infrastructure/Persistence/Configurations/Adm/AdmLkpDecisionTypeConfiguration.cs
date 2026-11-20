using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpDecisionTypeConfiguration : IEntityTypeConfiguration<AdmLkpDecisionType>
{
    public void Configure(EntityTypeBuilder<AdmLkpDecisionType> builder)
    {
        builder.ToTable("ADM_LKP_DECISION_TYPES");
        builder.HasKey(x => x.DecisionCode);

        builder.Property(x => x.DecisionCode).HasMaxLength(30);
        builder.Property(x => x.DecisionNameAr).HasMaxLength(200);
        builder.Property(x => x.DecisionNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
