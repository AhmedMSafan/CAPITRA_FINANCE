using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalInterfaceBatchConfiguration : IEntityTypeConfiguration<GlJournalInterfaceBatch>
{
    public void Configure(EntityTypeBuilder<GlJournalInterfaceBatch> builder)
    {
        builder.ToTable("GL_JOURNAL_INTERFACE_BATCHES");
        builder.HasKey(x => x.InterfaceBatchId);
        builder.HasIndex(x => new { x.LedgerId, x.InterfaceBatchNo }).IsUnique();

        builder.Property(x => x.InterfaceBatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InterfaceBatchNo).HasMaxLength(80);
        builder.Property(x => x.SourceBatchRef).HasMaxLength(120);
        builder.Property(x => x.BatchStatus).HasMaxLength(30);
        builder.Property(x => x.ImportedBy).HasMaxLength(255);
        builder.Property(x => x.ValidatedBy).HasMaxLength(255);
        builder.Property(x => x.ProcessedBy).HasMaxLength(255);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.GeneratedBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
