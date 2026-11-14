using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodAccessExceptionConfiguration : IEntityTypeConfiguration<GlPeriodAccessException>
{
    public void Configure(EntityTypeBuilder<GlPeriodAccessException> builder)
    {
        builder.ToTable("GL_PERIOD_ACCESS_EXCEPTIONS");
        builder.HasKey(x => x.PeriodExceptionId);
        builder.HasIndex(x => new { x.LedgerPeriodId, x.ExceptionCode }).IsUnique();
        builder.HasIndex(x => new { x.LedgerPeriodId, x.IsActive, x.ApprovalStatus });
        builder.HasIndex(x => new { x.AllowedUserName, x.AllowedRoleCode, x.IsActive });

        builder.Property(x => x.PeriodExceptionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ExceptionCode).HasMaxLength(60);
        builder.Property(x => x.ExceptionType).HasMaxLength(30);
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.AllowedUserName).HasMaxLength(100);
        builder.Property(x => x.AllowedRoleCode).HasMaxLength(60);
        builder.Property(x => x.MaxTotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.UsedTotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovalStatus).HasMaxLength(30);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.Reason).HasMaxLength(1000);

        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
