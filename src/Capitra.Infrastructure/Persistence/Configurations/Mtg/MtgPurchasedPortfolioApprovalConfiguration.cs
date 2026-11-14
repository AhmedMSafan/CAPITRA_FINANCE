using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgPurchasedPortfolioApprovalConfiguration : IEntityTypeConfiguration<MtgPurchasedPortfolioApproval>
{
    public void Configure(EntityTypeBuilder<MtgPurchasedPortfolioApproval> builder)
    {
        builder.ToTable("MTG_PURCHASED_PORTFOLIO_APPROVALS");
        builder.HasKey(x => x.ApprovalId);
        builder.HasIndex(x => x.PortfolioId);

        builder.Property(x => x.ApprovalId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.AssignedTo).HasMaxLength(255);
        builder.Property(x => x.DecisionCode).HasMaxLength(30);
        builder.Property(x => x.ReturnToStepCode).HasMaxLength(50);

        builder.HasOne<AdmLkpDecisionType>().WithMany().HasForeignKey(x => x.DecisionCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgPurchasedPortfolio>().WithMany().HasForeignKey(x => x.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpApprovalStep>().WithMany().HasForeignKey(x => x.ReturnToStepCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MtgLkpApprovalStep>().WithMany().HasForeignKey(x => x.StepCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
