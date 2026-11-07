using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffAppealConfiguration : IEntityTypeConfiguration<MffAppeal>
{
    public void Configure(EntityTypeBuilder<MffAppeal> builder)
    {
        builder.ToTable("MFF_APPEALS");
        builder.HasKey(x => x.AppealId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.DecisionCode);
        builder.HasIndex(x => x.RejectionDecisionId);

        builder.Property(x => x.AppealId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ReasonAr).HasMaxLength(2000);
        builder.Property(x => x.DecisionCode).HasMaxLength(30);
        builder.Property(x => x.DecisionReasonAr).HasMaxLength(2000);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDecisionCode>().WithMany().HasForeignKey(x => x.DecisionCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffCreditDecision>().WithMany().HasForeignKey(x => x.RejectionDecisionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
