using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Adm;

public sealed class AdmLkpChequeStatusConfiguration : IEntityTypeConfiguration<AdmLkpChequeStatus>
{
    public void Configure(EntityTypeBuilder<AdmLkpChequeStatus> builder)
    {
        builder.ToTable("ADM_LKP_CHEQUE_STATUSES");
        builder.HasKey(x => x.ChequeStatusCode);

        builder.Property(x => x.ChequeStatusCode).HasMaxLength(20);
        builder.Property(x => x.ChequeStatusAr).HasMaxLength(100);
        builder.Property(x => x.ChequeStatusEn).HasMaxLength(100);

        builder.ConfigureAudit();
    }
}
