using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffCreditDecisionConfiguration : IEntityTypeConfiguration<MffCreditDecision>
{
    public void Configure(EntityTypeBuilder<MffCreditDecision> builder)
    {
        builder.ToTable("MFF_CREDIT_DECISIONS");
        builder.HasKey(x => x.DecisionId);
        builder.HasIndex(x => x.DecisionCode);
        builder.HasIndex(x => x.ApplicationId);

        builder.Property(x => x.DecisionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DecisionCode).HasMaxLength(50);
        builder.Property(x => x.ApprovedFinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ConditionsText).HasMaxLength(2000);
        builder.Property(x => x.DecidedBy).HasMaxLength(100);
        builder.Property(x => x.CommitteeReference).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDecisionCode>().WithMany().HasForeignKey(x => x.DecisionCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
