using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodSourceStatusConfiguration : IEntityTypeConfiguration<GlPeriodSourceStatus>
{
    public void Configure(EntityTypeBuilder<GlPeriodSourceStatus> builder)
    {
        builder.ToTable("GL_PERIOD_SOURCE_STATUSES");
        builder.HasKey(x => x.SourceStatusId);
        builder.HasIndex(x => new { x.LedgerPeriodId, x.SourceCode, x.BusinessLineCode }).IsUnique();
        builder.HasIndex(x => new { x.SourceCode, x.SourceStatus });
        builder.HasIndex(x => new { x.LedgerPeriodId, x.SourceStatus, x.TransferStatus });

        builder.Property(x => x.SourceStatusId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.BusinessLineCode).HasMaxLength(30);
        builder.Property(x => x.SourceStatus).HasMaxLength(30);
        builder.Property(x => x.TransferStatus).HasMaxLength(30);
        builder.Property(x => x.ResponsibleRoleCode).HasMaxLength(60);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
