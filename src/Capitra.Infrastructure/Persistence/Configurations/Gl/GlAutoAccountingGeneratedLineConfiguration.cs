using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoAccountingGeneratedLineConfiguration : IEntityTypeConfiguration<GlAutoAccountingGeneratedLine>
{
    public void Configure(EntityTypeBuilder<GlAutoAccountingGeneratedLine> builder)
    {
        builder.ToTable("GL_AUTO_ACCOUNTING_GENERATED_LINES");
        builder.HasKey(x => x.GeneratedLineId);
        builder.HasIndex(x => new { x.GenerationId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.GenerationId, x.ValidationStatus });
        builder.HasIndex(x => new { x.AccountCombinationId, x.AccountId });

        builder.Property(x => x.GeneratedLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DrCrFlag).HasMaxLength(2);
        builder.Property(x => x.CombinationCode).HasMaxLength(240);
        builder.Property(x => x.EnteredCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredAmount).HasPrecision(18, 2);
        builder.Property(x => x.AccountedCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.AccountedAmount).HasPrecision(18, 2);
        builder.Property(x => x.ConversionRate).HasPrecision(9, 6);
        builder.Property(x => x.AmountCalculationTrace).HasMaxLength(2000);
        builder.Property(x => x.AccountDerivationTrace).HasMaxLength(2000);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.ValidationStatus).HasMaxLength(30);
        builder.Property(x => x.ValidationErrorCode).HasMaxLength(60);
        builder.Property(x => x.ValidationErrorMsg).HasMaxLength(4000);

        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.AccountedCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.EnteredCurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingGeneration>().WithMany().HasForeignKey(x => x.GenerationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlJournalInterfaceLine>().WithMany().HasForeignKey(x => x.InterfaceLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlRateType>().WithMany().HasForeignKey(x => x.RateTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingRuleLine>().WithMany().HasForeignKey(x => x.RuleLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
