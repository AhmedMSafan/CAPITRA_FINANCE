using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpUnitTypeConfiguration : LookupConfiguration<MtgLkpUnitType>
{
    protected override string TableName => "MTG_LKP_UNIT_TYPES";

    protected override int NameMaxLength => 100;
}
