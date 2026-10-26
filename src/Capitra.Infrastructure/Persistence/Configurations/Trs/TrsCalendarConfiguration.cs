using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsCalendarConfiguration : IEntityTypeConfiguration<TrsCalendar>
{
    public void Configure(EntityTypeBuilder<TrsCalendar> builder)
    {
        builder.ToTable("TRS_CALENDARS");
        builder.HasKey(x => x.CalendarId);
        builder.HasIndex(x => new { x.CompanyId, x.CalendarDate }).IsUnique();
        builder.HasIndex(x => new { x.CalendarDate, x.IsBusinessDay });

        builder.Property(x => x.CalendarId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.HolidayNameEn).HasMaxLength(160);
        builder.Property(x => x.HolidayNameAr).HasMaxLength(160);

        // COMPANY_ID -> TRS_COMPANIES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
