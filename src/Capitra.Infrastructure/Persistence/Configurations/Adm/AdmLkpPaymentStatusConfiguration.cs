using Capitra.Domain.Modules.Adm;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpPaymentStatusConfiguration : LookupConfiguration<AdmLkpPaymentStatus>
{
    protected override string TableName => "ADM_LKP_PAYMENT_STATUSES";

    protected override int NameMaxLength => 100;
}
