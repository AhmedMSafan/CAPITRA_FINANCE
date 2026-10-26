using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoAccountingRuleLineConfiguration : IEntityTypeConfiguration<GlAutoAccountingRuleLine>
{
    public void Configure(EntityTypeBuilder<GlAutoAccountingRuleLine> builder)
    {
        builder.ToTable("GL_AUTO_ACCOUNTING_RULE_LINES");
        builder.HasKey(x => x.RuleLineId);
        builder.HasIndex(x => new { x.RuleSetId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.RuleSetId, x.IsActive });
        builder.HasIndex(x => x.AmountSourceValueId);
        builder.HasIndex(x => new { x.FixedAccountCombinationId, x.FixedAccountId });

        builder.Property(x => x.RuleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.LineCode).HasMaxLength(80);
        builder.Property(x => x.LineNameEn).HasMaxLength(240);
        builder.Property(x => x.DrCrFlag).HasMaxLength(2);
        builder.Property(x => x.LineRoleCode).HasMaxLength(60);
        builder.Property(x => x.AccountDerivationMethod).HasMaxLength(40);
        builder.Property(x => x.AmountCalculationMethod).HasMaxLength(40);
        builder.Property(x => x.AmountValueCode).HasMaxLength(60);
        builder.Property(x => x.FixedAmount).HasPrecision(18, 2);
        builder.Property(x => x.FixedAmountCurrency).HasMaxLength(3);
        builder.Property(x => x.PercentageBaseCode).HasMaxLength(60);
        builder.Property(x => x.PercentageRate).HasPrecision(9, 6);
        builder.Property(x => x.MinimumAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaximumAmount).HasPrecision(18, 2);
        builder.Property(x => x.SignHandling).HasMaxLength(30);
        builder.Property(x => x.RoundingMethod).HasMaxLength(30);
        builder.Property(x => x.CurrencyDerivationMethod).HasMaxLength(30);
        builder.Property(x => x.FixedCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.LineDescriptionTemplate).HasMaxLength(1000);

        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.AccountSourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.FixedAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.AmountSourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.FixedAccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.CurrencySourceValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.FixedCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.FixedAmountCurrency).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingAccountLookup>().WithMany().HasForeignKey(x => x.AccountLookupId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlSourceValueDefinition>().WithMany().HasForeignKey(x => x.PercentageBaseValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingRuleSet>().WithMany().HasForeignKey(x => x.RuleSetId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
