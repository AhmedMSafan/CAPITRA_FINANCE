using Capitra.Domain.Modules.Tls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Tls;

public sealed class TlsLoanScenarioConfiguration : IEntityTypeConfiguration<TlsLoanScenario>
{
    public void Configure(EntityTypeBuilder<TlsLoanScenario> builder)
    {
        builder.ToTable("TLS_LOAN_SCENARIOS");
        builder.HasKey(x => x.ScenarioId);
        builder.HasIndex(x => new { x.CreatedBy, x.ScenarioName }).IsUnique();
        builder.Property(x => x.ScenarioId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ScenarioName).HasMaxLength(200);
        builder.Property(x => x.TenorUnit).HasMaxLength(20);
        builder.Property(x => x.ScheduleMode).HasMaxLength(20);
        builder.Property(x => x.PrincipalMethod).HasMaxLength(30);
        builder.Property(x => x.InterestPaymentMethod).HasMaxLength(20);
        builder.Property(x => x.DayCountMethod).HasMaxLength(20);
        builder.Property(x => x.PrincipalFrequencyUnit).HasMaxLength(20);
        builder.Property(x => x.PrincipalDateRule).HasMaxLength(20);
        builder.Property(x => x.InterestFrequencyUnit).HasMaxLength(20);
        builder.Property(x => x.InterestDateRule).HasMaxLength(20);
        builder.Property(x => x.CombinedFrequencyUnit).HasMaxLength(20);
        builder.Property(x => x.CombinedDateRule).HasMaxLength(20);
        builder.ConfigureAudit();
    }
}

public sealed class TlsLoanScenarioRateConfiguration : IEntityTypeConfiguration<TlsLoanScenarioRate>
{
    public void Configure(EntityTypeBuilder<TlsLoanScenarioRate> builder)
    {
        builder.ToTable("TLS_LOAN_SCENARIO_RATES");
        builder.HasKey(x => x.ScenarioRateId);
        builder.HasIndex(x => new { x.ScenarioId, x.EffectiveDate }).IsUnique();
        builder.Property(x => x.ScenarioRateId).UseIdentityByDefaultColumn();
        builder.HasOne<TlsLoanScenario>().WithMany().HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class TlsLoanScenarioLineConfiguration : IEntityTypeConfiguration<TlsLoanScenarioLine>
{
    public void Configure(EntityTypeBuilder<TlsLoanScenarioLine> builder)
    {
        builder.ToTable("TLS_LOAN_SCENARIO_LINES");
        builder.HasKey(x => x.ScheduleLineId);
        builder.HasIndex(x => new { x.ScenarioId, x.InstallmentNo }).IsUnique();
        builder.Property(x => x.ScheduleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DayCountFraction).HasPrecision(18, 12);
        builder.HasOne<TlsLoanScenario>().WithMany().HasForeignKey(x => x.ScenarioId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
