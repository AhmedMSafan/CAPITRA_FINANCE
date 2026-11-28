using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCalendarConfiguration : IEntityTypeConfiguration<GlCalendar>
{
    public void Configure(EntityTypeBuilder<GlCalendar> builder)
    {
        builder.ToTable("GL_CALENDARS");
        builder.HasKey(x => x.CalendarId);
        builder.HasIndex(x => x.CalendarCode).IsUnique();

        builder.Property(x => x.CalendarId).UseIdentityByDefaultColumn();
        builder.Property(x => x.CalendarCode).HasMaxLength(30);
        builder.Property(x => x.CalendarNameEn).HasMaxLength(150);
        builder.Property(x => x.CalendarNameAr).HasMaxLength(150);
        builder.Property(x => x.CalendarType).HasMaxLength(30);
        builder.Property(x => x.PeriodFrequency).HasMaxLength(30);

        builder.ConfigureAudit();
    }
}
