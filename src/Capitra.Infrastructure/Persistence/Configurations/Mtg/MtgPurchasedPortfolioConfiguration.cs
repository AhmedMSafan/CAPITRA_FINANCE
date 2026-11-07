using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolio>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolio> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIOS");
        builder.HasKey(x => x.PortfolioId);
        builder.HasIndex(x => x.PortfolioNo).IsUnique();
        builder.HasIndex(x => x.DeveloperId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.PortfolioStatusCode);
        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.WorkflowTaskId);

        builder.Property(x => x.PortfolioId).UseIdentityByDefaultColumn();
        builder.Property(x => x.PortfolioNo).HasMaxLength(30);
        builder.Property(x => x.PortfolioStatusCode).HasMaxLength(30);
        builder.Property(x => x.FaceValue).HasPrecision(18, 2);
        builder.Property(x => x.OutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.ProposedPurchaseValue).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedReturnAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdminExpenseAmount).HasPrecision(18, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.CurrentApprovalStep).HasMaxLength(50);
        builder.Property(x => x.BoardDecisionNo).HasMaxLength(100);
        builder.Property(x => x.BoardDecision).HasMaxLength(30);
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);
        builder.Property(x => x.SelectedFaceValue).HasPrecision(18, 2);
        builder.Property(x => x.SelectedOutstandingValue).HasPrecision(18, 2);
        builder.Property(x => x.SelectedPaidValue).HasPrecision(18, 2);
        builder.Property(x => x.LateFeeRate).HasPrecision(9, 6);
        builder.Property(x => x.PvRate).HasPrecision(9, 6);
        builder.Property(x => x.ReservedAmountPercent).HasPrecision(18, 6);
        builder.Property(x => x.CoveragePct).HasPrecision(9, 6);
        builder.Property(x => x.MarginPercentage).HasPrecision(18, 6);
        builder.Property(x => x.AdminExpensePercent).HasPrecision(9, 4);
        builder.Property(x => x.LegalFeePerUnit).HasPrecision(18, 2);
        builder.Property(x => x.ValuationFeePerUnit).HasPrecision(18, 2);

        builder.HasOne<AdmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpDecisionType>().WithMany().HasForeignKey(x => x.BoardDecision).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgDeveloper>().WithMany().HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgProject>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpApprovalStep>().WithMany().HasForeignKey(x => x.CurrentApprovalStep).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpPortfolioStatus>().WithMany().HasForeignKey(x => x.PortfolioStatusCode).OnDelete(DeleteBehavior.Restrict);
        // WORKFLOW_INSTANCE_ID -> WFE_WORKFLOW_INSTANCES: added when the WFE tables are ported.
        // WORKFLOW_TASK_ID -> WFE_WORKFLOW_TASK_QUEUE: added when the WFE tables are ported.
        builder.ConfigureAudit();
    }
}
