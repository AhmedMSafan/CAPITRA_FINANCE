using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpChequeStatusConfiguration : LookupConfiguration<AdmLkpChequeStatus>
{
    protected override string TableName => "ADM_LKP_CHEQUE_STATUS";

    protected override int NameMaxLength => 100;
}
