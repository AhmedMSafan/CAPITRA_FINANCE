using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpChargeTypeConfiguration : LookupConfiguration<AdmLkpChargeType>
{
    protected override string TableName => "ADM_LKP_CHARGE_TYPES";

    protected override int NameMaxLength => 100;
}
