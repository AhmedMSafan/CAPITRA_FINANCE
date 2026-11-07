using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalNumberingRuleConfiguration : IEntityTypeConfiguration<GlJournalNumberingRule>
{
    public void Configure(EntityTypeBuilder<GlJournalNumberingRule> builder)
    {
        builder.ToTable("GL_JOURNAL_NUMBERING_RULES");
        builder.HasKey(x => x.NumberingRuleId);
        builder.HasIndex(x => new { x.LedgerId, x.SourceId, x.CategoryId, x.JournalType }).IsUnique();

        builder.Property(x => x.NumberingRuleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.JournalType).HasMaxLength(30);
        builder.Property(x => x.Prefix).HasMaxLength(30);
        builder.Property(x => x.ResetFrequency).HasMaxLength(30);

        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
