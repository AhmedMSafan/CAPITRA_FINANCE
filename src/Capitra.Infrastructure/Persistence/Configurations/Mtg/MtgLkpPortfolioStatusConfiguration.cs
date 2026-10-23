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
        // (APEX FNV_PURSHASED_PORTFOLIO_PKG.create_portfolio) and the Excel import (page 208) ends in DATA_LOADING_COMPLETED.
        var seededAt = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new MtgLkpPortfolioStatus
            {
                Code = MtgPortfolio.DraftStatus,
                NameEn = "Draft",
                NameAr = "مسودة",
                DisplayOrder = 1,
                IsActive = true,
                CreatedBy = AuditInterceptor.SystemUser,
                CreatedAt = seededAt,
                RowVersion = 1
            },
            new MtgLkpPortfolioStatus
            {
                Code = MtgPortfolio.DataLoadingCompletedStatus,
                NameEn = "Data Loading Completed",
                NameAr = "اكتمل تحميل البيانات",
                DisplayOrder = 2,
                IsActive = true,
                CreatedBy = AuditInterceptor.SystemUser,
                CreatedAt = seededAt,
                RowVersion = 1
            });
    }
}
