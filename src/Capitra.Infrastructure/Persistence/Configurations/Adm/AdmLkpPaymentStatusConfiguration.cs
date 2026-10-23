using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpPaymentStatusConfiguration : LookupConfiguration<AdmLkpPaymentStatus>
{
    protected override string TableName => "ADM_LKP_PAYMENT_STATUS";

    protected override int NameMaxLength => 100;
}
