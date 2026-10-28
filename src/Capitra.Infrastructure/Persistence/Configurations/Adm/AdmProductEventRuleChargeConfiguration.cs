using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmProductEventRuleChargeConfiguration : IEntityTypeConfiguration<AdmProductEventRuleCharge>
{
    public void Configure(EntityTypeBuilder<AdmProductEventRuleCharge> builder)
    {
        builder.ToTable("ADM_PRODUCT_EVENT_RULE_CHARGES");
        builder.HasKey(x => x.RuleChargeId);
        builder.HasIndex(x => new { x.ProductEventRuleId, x.ChargeId }).IsUnique();
        builder.HasIndex(x => x.ChargeId);

        builder.Property(x => x.RuleChargeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CalculationMethodCode).HasMaxLength(30);
        builder.Property(x => x.ChargeTimingCode).HasMaxLength(30);
        builder.Property(x => x.AmountBaseCode).HasMaxLength(30);
        builder.Property(x => x.IncomeRecognitionCode).HasMaxLength(30).HasDefaultValue("PRORATE");

        builder.HasOne<AdmCharge>().WithMany().HasForeignKey(x => x.ChargeId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
