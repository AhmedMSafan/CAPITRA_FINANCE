using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctOfferConfiguration : IEntityTypeConfiguration<FctOffer>
{
    public void Configure(EntityTypeBuilder<FctOffer> builder)
    {
        builder.ToTable("FCT_OFFERS");
        // FCT_OFFERS has no primary key in Oracle; OFFER_ID is the key FINOVA_FINANCEING gives it (identity).
        builder.HasKey(x => x.OfferId);
        builder.Property(x => x.OfferId).UseIdentityByDefaultColumn();
        builder.HasIndex(x => new { x.ClientId, x.CurrencyId, x.OfferAt });

        builder.Property(x => x.OfferRef).HasMaxLength(30);
        builder.Property(x => x.FactoringType).HasMaxLength(40);
        builder.Property(x => x.RecourseType).HasMaxLength(40);
        builder.Property(x => x.ProductCode).HasMaxLength(40);
        builder.Property(x => x.RequestedLimit).HasPrecision(18, 3);
        builder.Property(x => x.AdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MaxAdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MinAdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ReserveRatePct).HasPrecision(9, 6);
        builder.Property(x => x.BaseRatePct).HasPrecision(9, 6);
        builder.Property(x => x.SpreadPct).HasPrecision(9, 6);
        builder.Property(x => x.EffectiveRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MarginPct).HasPrecision(9, 6);
        builder.Property(x => x.PenaltyRatePct).HasPrecision(9, 6);
        builder.Property(x => x.DayCountConvention).HasMaxLength(20);
        builder.Property(x => x.InterestCalcMethod).HasMaxLength(30);
        builder.Property(x => x.InterestFreq).HasMaxLength(20);
        builder.Property(x => x.MaxDebtorConcentrationPct).HasPrecision(9, 6);
        builder.Property(x => x.MaxSingleInvoicePct).HasPrecision(9, 6);
        builder.Property(x => x.MaxSingleInvoiceAmt).HasPrecision(18, 3);
        builder.Property(x => x.MinInvoiceAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaxDilutionPct).HasPrecision(9, 6);
        builder.Property(x => x.DilutionReservePct).HasPrecision(9, 6);
        builder.Property(x => x.ApprovedBy).HasMaxLength(200);
        builder.Property(x => x.RelationshipManager).HasMaxLength(200);
        builder.Property(x => x.RiskBand).HasMaxLength(10);
        builder.Property(x => x.Attribute2).HasMaxLength(500);
        builder.Property(x => x.Attribute3).HasMaxLength(500);
        builder.Property(x => x.Attribute4).HasMaxLength(500);
        builder.Property(x => x.Attribute5).HasMaxLength(500);
        builder.Property(x => x.StatusCode).HasMaxLength(30);
        builder.Property(x => x.MaxLimitTenorMth).HasPrecision(28, 2);
        builder.Property(x => x.AgreementNo).HasMaxLength(50);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
