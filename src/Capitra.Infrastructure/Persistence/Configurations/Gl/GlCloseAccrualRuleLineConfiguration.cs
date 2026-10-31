using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccrualRuleLineConfiguration : IEntityTypeConfiguration<GlCloseAccrualRuleLine>
{
    public void Configure(EntityTypeBuilder<GlCloseAccrualRuleLine> builder)
    {
        builder.ToTable("GL_CLOSE_ACCRUAL_RULE_LINES");
        builder.HasKey(x => x.AccrualRuleLineId);
        builder.HasIndex(x => new { x.AccrualRuleId, x.LineNo }).IsUnique();

        builder.Property(x => x.AccrualRuleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DrCrFlag).HasMaxLength(2);
        builder.Property(x => x.AccountDerivationMethod).HasMaxLength(30);
        builder.Property(x => x.AccountLookupCode).HasMaxLength(50);
        builder.Property(x => x.AmountMethod).HasMaxLength(30);
        builder.Property(x => x.AmountSourceCode).HasMaxLength(100);
        builder.Property(x => x.FixedAmount).HasPrecision(18, 2);
        builder.Property(x => x.PercentageValue).HasPrecision(28, 2);
        builder.Property(x => x.CurrencyDerivation).HasMaxLength(30);
        builder.Property(x => x.RoundingRule).HasMaxLength(30);
        builder.Property(x => x.LineDescriptionTemplate).HasMaxLength(1000);

        builder.HasOne<GlCloseAccrualRule>().WithMany().HasForeignKey(x => x.AccrualRuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
