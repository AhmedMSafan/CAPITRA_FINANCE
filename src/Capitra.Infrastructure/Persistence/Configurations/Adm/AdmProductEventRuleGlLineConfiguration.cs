using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmProductEventRuleGlLineConfiguration : IEntityTypeConfiguration<AdmProductEventRuleGlLine>
{
    public void Configure(EntityTypeBuilder<AdmProductEventRuleGlLine> builder)
    {
        builder.ToTable("ADM_PRODUCT_EVENT_RULE_GL_LINES");
        builder.HasKey(x => x.RuleGlLineId);
        builder.HasIndex(x => new { x.ProductEventRuleId, x.RuleChargeId, x.JournalNo, x.LineNo }).IsUnique().AreNullsDistinct(false);
        builder.HasIndex(x => x.RuleChargeId);
        builder.HasIndex(x => x.AccountId);

        builder.Property(x => x.RuleGlLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DebitCreditCode).HasMaxLength(1);
        builder.Property(x => x.JournalNo).HasDefaultValue(1);
        builder.Property(x => x.AmountCode).HasMaxLength(50);
        builder.Property(x => x.AccountSourceCode).HasMaxLength(30).HasDefaultValue(AdmProductEventRuleGlLine.AccountSourceAccount);
        builder.Property(x => x.DescriptionTemplate).HasMaxLength(500);

        builder.HasOne<AdmProductEventRuleCharge>().WithMany().HasForeignKey(x => x.RuleChargeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
