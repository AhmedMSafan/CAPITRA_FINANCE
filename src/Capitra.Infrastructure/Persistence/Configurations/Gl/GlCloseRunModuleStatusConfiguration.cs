using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseRunModuleStatusConfiguration : IEntityTypeConfiguration<GlCloseRunModuleStatus>
{
    public void Configure(EntityTypeBuilder<GlCloseRunModuleStatus> builder)
    {
        builder.ToTable("GL_CLOSE_RUN_MODULE_STATUSES");
        builder.HasKey(x => x.RunModuleStatusId);
        builder.HasIndex(x => new { x.CloseRunId, x.SourceModule }).IsUnique();
        builder.HasIndex(x => new { x.CloseRunId, x.ModuleStatus, x.PostingStatus });

        builder.Property(x => x.RunModuleStatusId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceModuleNameEn).HasMaxLength(200);
        builder.Property(x => x.ModuleStatus).HasMaxLength(30);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.ReconciliationStatus).HasMaxLength(30);
        builder.Property(x => x.TotalTransactions).HasPrecision(28, 2);
        builder.Property(x => x.SourceControlTotal).HasPrecision(28, 2);
        builder.Property(x => x.GlControlTotal).HasPrecision(28, 2);
        builder.Property(x => x.VarianceAmount).HasPrecision(18, 2);
        builder.Property(x => x.CertifiedBy).HasMaxLength(100);
        builder.Property(x => x.SourceReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
