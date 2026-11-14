using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsFacilityCalendarDayConfiguration : IEntityTypeConfiguration<TrsFacilityCalendarDay>
{
    public void Configure(EntityTypeBuilder<TrsFacilityCalendarDay> builder)
    {
        builder.ToTable("TRS_FACILITY_CALENDAR_DAYS");
        builder.HasKey(x => x.CalendarDayId);
        builder.HasIndex(x => new { x.BankId, x.CalendarDate }).IsUnique();
        builder.HasIndex(x => new { x.CalendarDate, x.IsWorkingDay });

        builder.Property(x => x.CalendarDayId).UseIdentityByDefaultColumn();
        builder.Property(x => x.HolidayNameEn).HasMaxLength(240);
        builder.Property(x => x.HolidayNameAr).HasMaxLength(240);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<TrsBank>().WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
