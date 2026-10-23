using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpDdCheckTypeConfiguration : LookupConfiguration<MtgLkpDdCheckType>
{
    protected override string TableName => "MTG_LKP_DD_CHECK_TYPE";

    protected override int NameMaxLength => 300;
}
