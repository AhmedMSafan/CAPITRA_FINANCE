using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctDisbursementCustomScheduleConfiguration : IEntityTypeConfiguration<FctDisbursementCustomSchedule>
{
    public void Configure(EntityTypeBuilder<FctDisbursementCustomSchedule> builder)
    {
        builder.ToTable("FCT_DISBURSEMENT_CUSTOM_SCHEDULES");
        builder.HasKey(x => x.CustomScheduleId);
        builder.HasIndex(x => new { x.DisbursementId, x.DueDate }).IsUnique();

        builder.Property(x => x.CustomScheduleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PrincipalAmount).HasPrecision(18, 2);

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
