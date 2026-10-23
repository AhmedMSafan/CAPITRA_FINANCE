using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpValuationTypeConfiguration : LookupConfiguration<MtgLkpValuationType>
{
    protected override string TableName => "MTG_LKP_VALUATION_TYPE";
}
