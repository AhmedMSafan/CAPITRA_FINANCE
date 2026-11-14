using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Trs;

public sealed class TrsCreditFacilityConfiguration : IEntityTypeConfiguration<TrsCreditFacility>
{
    public void Configure(EntityTypeBuilder<TrsCreditFacility> builder)
    {
        builder.ToTable("TRS_CREDIT_FACILITIES");
        builder.HasKey(x => x.FacilityId);
        builder.HasIndex(x => new { x.CompanyId, x.FacilityRef }).IsUnique();
        builder.HasIndex(x => x.MaturityDate);
        builder.HasIndex(x => new { x.BankId, x.FacilityTypeCode });
        builder.HasIndex(x => new { x.CompanyId, x.FacilityStatus });

        builder.Property(x => x.FacilityId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FacilityRef).HasMaxLength(60);
        builder.Property(x => x.FacilityTypeCode).HasMaxLength(50);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.ApprovedLimit).HasPrecision(18, 3);
        builder.Property(x => x.OutstandingBalance).HasPrecision(18, 3);
        builder.Property(x => x.AvailableBalance).HasPrecision(18, 3);
        builder.Property(x => x.InterestRateType).HasMaxLength(20);
        builder.Property(x => x.BaseRateCode).HasMaxLength(50);
        builder.Property(x => x.MarginBps).HasPrecision(9, 3);
        builder.Property(x => x.AllInRatePct).HasPrecision(9, 6);
        builder.Property(x => x.AmortizationType).HasMaxLength(30);
        builder.Property(x => x.SecurityDescriptionEn).HasMaxLength(2000);
        builder.Property(x => x.SecurityDescriptionAr).HasMaxLength(2000);
        builder.Property(x => x.FacilityStatus).HasMaxLength(30);
        builder.Property(x => x.FraRegistrationRef).HasMaxLength(100);
        builder.Property(x => x.NfaUtilizedAmount).HasPrecision(18, 2);
        builder.Property(x => x.InterestDayBasisCode).HasMaxLength(30);
        builder.Property(x => x.RepaymentFrequencyCode).HasMaxLength(30);
        builder.Property(x => x.BankCollectionPolicyCode).HasMaxLength(30);
        builder.Property(x => x.AccrualFrequencyCode).HasMaxLength(30);
        builder.Property(x => x.AdjustmentPostingModeCode).HasMaxLength(30);
        builder.Property(x => x.ReservedBalance).HasPrecision(18, 3);

        // COMPANY_ID -> TRS_COMPANIES: added when that table is ported.
        builder.HasOne<TrsBank>().WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
        // CURRENCY_CODE -> TRS_CURRENCIES: added when that table is ported.
        builder.ConfigureAudit();
    }
}
