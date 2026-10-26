using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingScheduleConfiguration : IEntityTypeConfiguration<GlPostingSchedule>
{
    public void Configure(EntityTypeBuilder<GlPostingSchedule> builder)
    {
        builder.ToTable("GL_POSTING_SCHEDULES");
        builder.HasKey(x => x.PostingScheduleId);
        builder.HasIndex(x => x.ScheduleCode).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.SourceCode, x.IsActive, x.NextRunAt });

        builder.Property(x => x.PostingScheduleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ScheduleCode).HasMaxLength(50);
        builder.Property(x => x.ScheduleName).HasMaxLength(200);
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.CategoryCode).HasMaxLength(50);
        builder.Property(x => x.FrequencyCode).HasMaxLength(30);
        builder.Property(x => x.ScheduleTime).HasMaxLength(10);
        builder.Property(x => x.DayOfWeek).HasMaxLength(10);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlPostingProfile>().WithMany().HasForeignKey(x => x.PostingProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
