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

        // Every new portfolio starts as DRAFT (APEX FNV_PURSHASED_PORTFOLIO_PKG.create_portfolio), so the code must exist.
        builder.HasData(new MtgLkpPortfolioStatus
        {
            Code = MtgPortfolio.DraftStatus,
            NameEn = "Draft",
            NameAr = "مسودة",
            DisplayOrder = 1,
            IsActive = true,
            CreatedBy = AuditInterceptor.SystemUser,
            CreatedAt = new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc),
            RowVersion = 1
        });
    }
}
