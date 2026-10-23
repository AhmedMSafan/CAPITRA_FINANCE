using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpCurrencyConfiguration : LookupConfiguration<AdmLkpCurrency>
{
    protected override string TableName => "ADM_LKP_CURRENCY";
}
