using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgFinancialStudyConfiguration : IEntityTypeConfiguration<MtgFinancialStudy>
{
    public void Configure(EntityTypeBuilder<MtgFinancialStudy> builder)
    {
        builder.ToTable("MTG_FINANCIAL_STUDIES");
        builder.HasKey(x => x.FinStudyId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.FinStudyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PortfolioFaceValue).HasPrecision(18, 2);
        builder.Property(x => x.OutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.ProposedPurchaseValue).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedReturnAmount).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedReturnRate).HasPrecision(9, 6);
        builder.Property(x => x.AdminExpenseAmount).HasPrecision(18, 2);
        builder.Property(x => x.NetReturnAmount).HasPrecision(18, 2);
        builder.Property(x => x.ChequeTotalValue).HasPrecision(18, 2);
        builder.Property(x => x.ChequeCoveragePct).HasPrecision(9, 6);
        builder.Property(x => x.DepositAmount).HasPrecision(18, 2);
        builder.Property(x => x.DepositPct).HasPrecision(9, 6);
        builder.Property(x => x.PreparedBy).HasMaxLength(255);
        builder.Property(x => x.ApprovedBy).HasMaxLength(255);

        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
