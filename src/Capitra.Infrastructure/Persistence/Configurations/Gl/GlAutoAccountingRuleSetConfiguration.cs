using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoAccountingRuleSetConfiguration : IEntityTypeConfiguration<GlAutoAccountingRuleSet>
{
    public void Configure(EntityTypeBuilder<GlAutoAccountingRuleSet> builder)
    {
        builder.ToTable("GL_AUTO_ACCOUNTING_RULE_SETS");
        builder.HasKey(x => x.RuleSetId);
        builder.HasIndex(x => new { x.SourceId, x.EventTypeId, x.LedgerId, x.RuleSetCode, x.VersionNo }).IsUnique();
        builder.HasIndex(x => new { x.EventTypeId, x.RuleStatus, x.ApprovalStatus });
        builder.HasIndex(x => new { x.LedgerId, x.EffectiveFromDate, x.EffectiveToDate });
        builder.HasIndex(x => new { x.BusinessLineCode, x.ProductCode, x.TransactionTypeCode });

        builder.Property(x => x.RuleSetId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RuleSetCode).HasMaxLength(80);
        builder.Property(x => x.RuleSetNameEn).HasMaxLength(240);
        builder.Property(x => x.RuleSetNameAr).HasMaxLength(240);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.ProductCode).HasMaxLength(60);
        builder.Property(x => x.TransactionTypeCode).HasMaxLength(60);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.RuleStatus).HasMaxLength(30);
        builder.Property(x => x.ApprovalStatus).HasMaxLength(30);
        builder.Property(x => x.GenerationMode).HasMaxLength(30);
        builder.Property(x => x.ReversalBehavior).HasMaxLength(30);
        builder.Property(x => x.DuplicatePolicy).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.SubmittedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.DefaultCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountingEventType>().WithMany().HasForeignKey(x => x.EventTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinLegalEntity>().WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
