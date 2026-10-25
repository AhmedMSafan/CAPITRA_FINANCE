using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpGuaranteeTypeConfiguration : LookupConfiguration<AdmLkpGuaranteeType>
{
    protected override string TableName => "ADM_LKP_GUARANTEE_TYPES";

    protected override int NameMaxLength => 300;
}
