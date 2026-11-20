using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioIncomeScheduleConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioIncomeSchedule>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioIncomeSchedule> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES");
        builder.HasKey(x => x.IncomeLineId);
        builder.HasIndex(x => new { x.DisbursementId, x.DueDate }).IsUnique();

        builder.Property(x => x.IncomeLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FundingRecoveryAmount).HasPrecision(18, 2);
        builder.Property(x => x.MarginAmount).HasPrecision(18, 2);
        builder.Property(x => x.FeesAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PostingStatus).HasMaxLength(30);
        builder.Property(x => x.TranSetId).HasMaxLength(60);
        builder.Property(x => x.PostedBy).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
