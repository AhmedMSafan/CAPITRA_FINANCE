using Capitra.Domain.Modules.Mtg;
using Capitra.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpPortfolioStatusConfiguration : LookupConfiguration<MtgLkpPortfolioStatus>
{
    protected override string TableName => "MTG_LKP_PORTFOLIO_STATUS";

    public override void Configure(EntityTypeBuilder<MtgLkpPortfolioStatus> builder)
    {
        base.Configure(builder);

        // Statuses the services set themselves, so the codes must exist: every new portfolio starts as DRAFT
        // (APEX FNV_PURSHASED_PORTFOLIO_PKG.create_portfolio), and each workflow step page sets its own outcome.
        builder.HasData(
            Seed(MtgPortfolio.DraftStatus, "Draft", "مسودة", 1),
            Seed(MtgPortfolio.DataLoadingCompletedStatus, "Data Loading Completed", "اكتمل تحميل البيانات", 2),
            Seed(MtgPortfolio.AmlAcceptedStatus, "AML Accepted", "تم قبول مراجعة مكافحة غسل الأموال", 3),
            Seed(MtgPortfolio.AmlRejectedStatus, "AML Rejected", "تم رفض مراجعة مكافحة غسل الأموال", 4),
            Seed(MtgPortfolio.InquiryAcceptedStatus, "Inquiry Accepted", "تم قبول الاستعلامات", 5),
            Seed(MtgPortfolio.InquiryRejectedStatus, "Inquiry Rejected", "تم رفض الاستعلامات", 6),
            Seed(MtgPortfolio.LegalReviewAcceptedStatus, "Legal Review Accepted", "تم قبول المراجعة القانونية", 7),
            Seed(MtgPortfolio.LegalReviewRejectedStatus, "Legal Review Rejected", "تم رفض المراجعة القانونية", 8),
            Seed(MtgPortfolio.ValuationCompletedStatus, "Valuation Completed", "اكتمل التقييم", 9),
            Seed(MtgPortfolio.CoverageReviewAcceptedStatus, "Coverage Review Accepted", "تم قبول مراجعة التغطية", 10),
            Seed(MtgPortfolio.CoverageReviewRejectedStatus, "Coverage Review Rejected", "تم رفض مراجعة التغطية", 11),
            Seed(MtgPortfolio.ChequeCoverageAcceptedStatus, "Cheque Coverage Accepted", "تم قبول تغطية الشيكات", 12),
            Seed(MtgPortfolio.ChequeCoverageRejectedStatus, "Cheque Coverage Rejected", "تم رفض تغطية الشيكات", 13));
    }

    private static MtgLkpPortfolioStatus Seed(string code, string nameEn, string nameAr, int displayOrder) => new()
    {
        Code = code,
        NameEn = nameEn,
        NameAr = nameAr,
        DisplayOrder = displayOrder,
        IsActive = true,
        CreatedBy = AuditInterceptor.SystemUser,
        CreatedAt = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc),
        RowVersion = 1
    };
}
