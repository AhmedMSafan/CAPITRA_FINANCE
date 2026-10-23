using Capitra.Domain.Modules.Mtg;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpInquiryTypeConfiguration : LookupConfiguration<MtgLkpInquiryType>
{
    protected override string TableName => "MTG_LKP_INQUIRY_TYPE";
}
