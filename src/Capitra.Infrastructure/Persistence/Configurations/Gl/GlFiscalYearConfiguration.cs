using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlFiscalYearConfiguration : IEntityTypeConfiguration<GlFiscalYear>
{
    public void Configure(EntityTypeBuilder<GlFiscalYear> builder)
    {
        builder.ToTable("GL_FISCAL_YEARS");
        builder.HasKey(x => x.FiscalYearId);
        builder.HasIndex(x => new { x.CalendarId, x.FiscalYear }).IsUnique();
        builder.HasIndex(x => new { x.CalendarId, x.YearStatus });

        builder.Property(x => x.FiscalYearId).UseIdentityByDefaultColumn();
        builder.Property(x => x.YearName).HasMaxLength(50);
        builder.Property(x => x.YearStatus).HasMaxLength(30);

        builder.HasOne<GlCalendar>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
