using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpPortfolioStatusConfiguration : LookupConfiguration<MtgLkpPortfolioStatus>
{
    protected override string TableName => "MTG_LKP_PORTFOLIO_STATUS";
}
