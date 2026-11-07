using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioRepaymentScheduleConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioRepaymentSchedule>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioRepaymentSchedule> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULES");
        builder.HasKey(x => x.ScheduleLineId);
        builder.HasIndex(x => new { x.DisbursementId, x.SourceChequeId }).IsUnique();

        builder.Property(x => x.ScheduleLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequeNo).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(300);
        builder.Property(x => x.ChequeAmount).HasPrecision(18, 2);
        builder.Property(x => x.PvAmount).HasPrecision(18, 2);
        builder.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        builder.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(x => x.InterestAmount).HasPrecision(18, 2);
        builder.Property(x => x.ClosingBalance).HasPrecision(18, 2);
        builder.Property(x => x.MarginRelease).HasPrecision(18, 2);
        builder.Property(x => x.FeeRelease).HasPrecision(18, 2);
        builder.Property(x => x.EirRate).HasPrecision(9, 6);

        builder.ConfigureAudit();
    }
}
