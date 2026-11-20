using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingSequenceAuditConfiguration : IEntityTypeConfiguration<GlPostingSequenceAudit>
{
    public void Configure(EntityTypeBuilder<GlPostingSequenceAudit> builder)
    {
        builder.ToTable("GL_POSTING_SEQUENCE_AUDITS");
        builder.HasKey(x => x.PostingSequenceAuditId);
        builder.HasIndex(x => new { x.SequenceScope, x.LedgerId, x.LedgerPeriodId, x.GeneratedAt });

        builder.Property(x => x.PostingSequenceAuditId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SequenceScope).HasMaxLength(30);
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.GeneratedReference).HasMaxLength(100);
        builder.Property(x => x.RelatedTableName).HasMaxLength(100);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
