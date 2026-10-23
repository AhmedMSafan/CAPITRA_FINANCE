using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpGuaranteeTypeConfiguration : LookupConfiguration<AdmLkpGuaranteeType>
{
    protected override string TableName => "ADM_LKP_GUARANTEE_TYPE";

    protected override int NameMaxLength => 300;
}
