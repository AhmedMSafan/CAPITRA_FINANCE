using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseReportConfiguration : IEntityTypeConfiguration<GlCloseReport>
{
    public void Configure(EntityTypeBuilder<GlCloseReport> builder)
    {
        builder.ToTable("GL_CLOSE_REPORTS");
        builder.HasKey(x => x.CloseReportId);
        builder.HasIndex(x => new { x.CloseRunId, x.ReportCode }).IsUnique();
        builder.HasIndex(x => new { x.CloseRunId, x.ReportType, x.ReportStatus });

        builder.Property(x => x.CloseReportId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReportCode).HasMaxLength(50);
        builder.Property(x => x.ReportNameEn).HasMaxLength(200);
        builder.Property(x => x.ReportNameAr).HasMaxLength(200);
        builder.Property(x => x.ReportType).HasMaxLength(30);
        builder.Property(x => x.ReportStatus).HasMaxLength(30);
        builder.Property(x => x.GeneratedFileName).HasMaxLength(255);
        builder.Property(x => x.GeneratedFileMimeType).HasMaxLength(100);
        builder.Property(x => x.GeneratedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);

        builder.HasOne<GlCloseRun>().WithMany().HasForeignKey(x => x.CloseRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
