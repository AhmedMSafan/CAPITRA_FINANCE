using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffCalculationScheduleConfiguration : IEntityTypeConfiguration<MffCalculationSchedule>
{
    public void Configure(EntityTypeBuilder<MffCalculationSchedule> builder)
    {
        builder.ToTable("MFF_CALCULATION_SCHEDULES");
        builder.HasKey(x => x.CalcScheduleId);
        builder.HasIndex(x => new { x.CalcRunId, x.InstallmentNo }).IsUnique();
        builder.HasIndex(x => x.CalcRunId);

        builder.Property(x => x.CalcScheduleId).UseIdentityByDefaultColumn();
        builder.Property(x => x.OpeningPrincipal).HasPrecision(18, 2);
        builder.Property(x => x.BorrowerPayment).HasPrecision(18, 2);
        builder.Property(x => x.SubsidyPayment).HasPrecision(18, 2);
        builder.Property(x => x.BulletPayment).HasPrecision(18, 2);
        builder.Property(x => x.InterestAmount).HasPrecision(18, 2);
        builder.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ClosingPrincipal).HasPrecision(18, 2);

        builder.HasOne<MffCalculationRun>().WithMany().HasForeignKey(x => x.CalcRunId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
