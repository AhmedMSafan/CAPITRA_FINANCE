using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPeriodStatusHistoryConfiguration : IEntityTypeConfiguration<GlPeriodStatusHistory>
{
    public void Configure(EntityTypeBuilder<GlPeriodStatusHistory> builder)
    {
        builder.ToTable("GL_PERIOD_STATUS_HIST");
        builder.HasKey(x => x.PeriodHistoryId);

        builder.Property(x => x.PeriodHistoryId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OldPeriodStatus).HasMaxLength(30);
        builder.Property(x => x.NewPeriodStatus).HasMaxLength(30);
        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.ActionReason).HasMaxLength(1000);
        builder.Property(x => x.ActionBy).HasMaxLength(100);
        builder.Property(x => x.SourceSessionId).HasMaxLength(100);

        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
