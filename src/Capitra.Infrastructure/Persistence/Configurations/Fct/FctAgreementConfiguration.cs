using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctAgreementConfiguration : IEntityTypeConfiguration<FctAgreement>
{
    public void Configure(EntityTypeBuilder<FctAgreement> builder)
    {
        builder.ToTable("FCT_AGREEMENTS");
        builder.HasKey(x => x.AgreementId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => new { x.EffectiveAt, x.MaturityAt });
        builder.HasIndex(x => x.PreviousAgreementId);
        builder.HasIndex(x => x.RateTypeId);
        builder.HasIndex(x => x.RecourseType);
        builder.HasIndex(x => x.NextReviewDate);
        builder.HasIndex(x => x.RmEmployeeId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => new { x.AgreementType, x.FactoringType });
        builder.HasIndex(x => x.AgreementNumber).IsUnique();

        builder.Property(x => x.AgreementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.AgreementNumber).HasMaxLength(30);
        builder.Property(x => x.AgreementRef).HasMaxLength(50);
        builder.Property(x => x.AgreementName).HasMaxLength(500);
        builder.Property(x => x.AgreementNameAr).HasMaxLength(500);
        builder.Property(x => x.AgreementType).HasMaxLength(30);
        builder.Property(x => x.AgreementTypeAr).HasMaxLength(30);
        builder.Property(x => x.FactoringType).HasMaxLength(30);
        builder.Property(x => x.FactoringTypeAr).HasMaxLength(30);
        builder.Property(x => x.RecourseType).HasMaxLength(30);
        builder.Property(x => x.RecourseTypeAr).HasMaxLength(30);
        builder.Property(x => x.NotificationType).HasMaxLength(30);
        builder.Property(x => x.NotificationTypeAr).HasMaxLength(30);
        builder.Property(x => x.ProductCode).HasMaxLength(30);
        builder.Property(x => x.FacilityLimit).HasPrecision(18, 3);
        builder.Property(x => x.ApprovedLimit).HasPrecision(18, 3);
        builder.Property(x => x.UtilizedAmount).HasPrecision(18, 2);
        builder.Property(x => x.AvailableAmount).HasPrecision(18, 2);
        builder.Property(x => x.SublimitAmount).HasPrecision(18, 2);
        builder.Property(x => x.AdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MaxAdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.MinAdvanceRatePct).HasPrecision(9, 6);
        builder.Property(x => x.ReserveRatePct).HasPrecision(9, 6);
        builder.Property(x => x.BaseRatePct).HasPrecision(9, 6);
        builder.Property(x => x.SpreadPct).HasPrecision(9, 6);
        builder.Property(x => x.EffectiveRatePct).HasPrecision(28, 10);
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
        builder.Property(x => x.ApprovalAuthority).HasMaxLength(200);
        builder.Property(x => x.ApprovalAuthorityAr).HasMaxLength(200);
        builder.Property(x => x.ApprovalReference).HasMaxLength(100);
        builder.Property(x => x.RelationshipManager).HasMaxLength(200);
        builder.Property(x => x.OperationsOfficer).HasMaxLength(200);
        builder.Property(x => x.GoverningLaw).HasMaxLength(200);
        builder.Property(x => x.GoverningLawAr).HasMaxLength(200);
        builder.Property(x => x.Jurisdiction).HasMaxLength(200);
        builder.Property(x => x.JurisdictionAr).HasMaxLength(200);
        builder.Property(x => x.Attribute1).HasMaxLength(500);
        builder.Property(x => x.Attribute2).HasMaxLength(500);
        builder.Property(x => x.Attribute3).HasMaxLength(500);
        builder.Property(x => x.Attribute4).HasMaxLength(500);
        builder.Property(x => x.Attribute5).HasMaxLength(500);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.CreditBureauScore).HasPrecision(28, 2);
        builder.Property(x => x.InvestigationNotes).HasMaxLength(500);
        builder.Property(x => x.AmlRiskBand).HasMaxLength(20);
        builder.Property(x => x.AmlScreeningResult).HasMaxLength(20);
        builder.Property(x => x.AmlScreeningNotes).HasMaxLength(500);
        builder.Property(x => x.LegalOpenionNotes).HasMaxLength(500);
        builder.Property(x => x.FsAssessmentResult).HasMaxLength(20);
        builder.Property(x => x.RrAssessmentResult).HasMaxLength(20);
        builder.Property(x => x.RequiredLimit).HasPrecision(18, 3);
        builder.Property(x => x.RecommendedLimt).HasPrecision(18, 3);
        builder.Property(x => x.RecommendedAdvRate).HasPrecision(9, 6);
        builder.Property(x => x.CreditNotes).HasMaxLength(500);
        builder.Property(x => x.RiskNotes).HasMaxLength(500);
        builder.Property(x => x.RiskDecision).HasMaxLength(40);
        builder.Property(x => x.ApprovedAdvRate).HasPrecision(9, 6);
        builder.Property(x => x.FsNotes).HasMaxLength(500);
        builder.Property(x => x.RrNotes).HasMaxLength(500);
        builder.Property(x => x.AdditionalTermsConditions).HasMaxLength(200);
        builder.Property(x => x.CurrentStep).HasMaxLength(50);

        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmLkpCurrency>().WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctAgreement>().WithMany().HasForeignKey(x => x.PreviousAgreementId).OnDelete(DeleteBehavior.Restrict);
        // RATE_TYPE_ID -> LKP_INTEREST_RATE_TYPES: added when the LKP tables are ported.
        builder.HasOne<CrmLkpStatusCode>().WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
