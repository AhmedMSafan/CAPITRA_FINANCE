using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffCreditStudyConfiguration : IEntityTypeConfiguration<MffCreditStudy>
{
    public void Configure(EntityTypeBuilder<MffCreditStudy> builder)
    {
        builder.ToTable("MFF_CREDIT_STUDIES");
        builder.HasKey(x => x.CreditStudyId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.RecommendationCode);

        builder.Property(x => x.CreditStudyId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AnalystUser).HasMaxLength(100);
        builder.Property(x => x.VerifiedIncomeAmount).HasPrecision(18, 2);
        builder.Property(x => x.MonthlyObligationsAmount).HasPrecision(18, 2);
        builder.Property(x => x.DisposableIncomeAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProposedFinanceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProposedRatePct).HasPrecision(9, 6);
        builder.Property(x => x.InstallmentIncomePct).HasPrecision(9, 6);
        builder.Property(x => x.LtvPct).HasPrecision(9, 6);
        builder.Property(x => x.RecommendationCode).HasMaxLength(50);

        builder.HasOne<MffApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDecisionCode>().WithMany().HasForeignKey(x => x.RecommendationCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
