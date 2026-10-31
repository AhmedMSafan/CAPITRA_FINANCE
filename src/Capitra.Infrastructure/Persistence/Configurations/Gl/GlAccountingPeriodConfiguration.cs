using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAccountingPeriodConfiguration : IEntityTypeConfiguration<GlAccountingPeriod>
{
    public void Configure(EntityTypeBuilder<GlAccountingPeriod> builder)
    {
        builder.ToTable("GL_ACCOUNTING_PERIODS");
        builder.HasKey(x => x.PeriodId);
        builder.HasIndex(x => new { x.CalendarId, x.PeriodCode }).IsUnique();
        builder.HasIndex(x => new { x.FiscalYearId, x.PeriodNo }).IsUnique();
        builder.HasIndex(x => new { x.CalendarId, x.PeriodStatus, x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.FiscalYearId, x.PeriodStatus });

        builder.Property(x => x.PeriodId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PeriodCode).HasMaxLength(30);
        builder.Property(x => x.PeriodNameEn).HasMaxLength(80);
        builder.Property(x => x.PeriodNameAr).HasMaxLength(80);
        builder.Property(x => x.PeriodStatus).HasMaxLength(30);
        builder.Property(x => x.OpenedBy).HasMaxLength(100);
        builder.Property(x => x.ClosedBy).HasMaxLength(100);

        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlFiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
