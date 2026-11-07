using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoAccountingGenerationConfiguration : IEntityTypeConfiguration<GlAutoAccountingGeneration>
{
    public void Configure(EntityTypeBuilder<GlAutoAccountingGeneration> builder)
    {
        builder.ToTable("GL_AUTO_ACCOUNTING_GENERATIONS");
        builder.HasKey(x => x.GenerationId);
        builder.HasIndex(x => x.GenerationNo).IsUnique();
        builder.HasIndex(x => new { x.AccountingEventId, x.RuleSetId }).IsUnique();

        builder.Property(x => x.GenerationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.GenerationNo).HasMaxLength(80);
        builder.Property(x => x.GenerationStatus).HasMaxLength(30);
        builder.Property(x => x.GeneratedBy).HasMaxLength(255);
        builder.Property(x => x.TotalEnteredDr).HasPrecision(22, 5);
        builder.Property(x => x.TotalEnteredCr).HasPrecision(22, 5);
        builder.Property(x => x.TotalAccountedDr).HasPrecision(22, 5);
        builder.Property(x => x.TotalAccountedCr).HasPrecision(22, 5);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlAccountingEvent>().WithMany().HasForeignKey(x => x.AccountingEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalInterfaceBatch>().WithMany().HasForeignKey(x => x.InterfaceBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAutoAccountingRuleSet>().WithMany().HasForeignKey(x => x.RuleSetId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
