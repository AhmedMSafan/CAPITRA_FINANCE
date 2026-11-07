using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodPostingWindowConfiguration : IEntityTypeConfiguration<GlPeriodPostingWindow>
{
    public void Configure(EntityTypeBuilder<GlPeriodPostingWindow> builder)
    {
        builder.ToTable("GL_PERIOD_POSTING_WINDOWS");
        builder.HasKey(x => x.PostingWindowId);
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.SourceCode, x.CategoryCode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.IsActive });

        builder.Property(x => x.PostingWindowId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.CategoryCode).HasMaxLength(50);
        builder.Property(x => x.ReopenedBy).HasMaxLength(100);
        builder.Property(x => x.ReopenReason).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
