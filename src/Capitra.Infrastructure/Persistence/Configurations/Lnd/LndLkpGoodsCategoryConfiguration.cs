using Capitra.Domain.Modules.Lnd;

namespace Capitra.Infrastructure.Persistence.Configurations.Lnd;

public sealed class LndLkpGoodsCategoryConfiguration : LookupConfiguration<LndLkpGoodsCategory>
{
    protected override string TableName => "LND_LKP_GOODS_CATEGORIES";

    protected override int NameMaxLength => 100;
}
