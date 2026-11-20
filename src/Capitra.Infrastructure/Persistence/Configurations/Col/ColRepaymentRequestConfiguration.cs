using Capitra.Domain.Modules.Col;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Col;

public sealed class ColRepaymentRequestConfiguration : IEntityTypeConfiguration<ColRepaymentRequest>
{
    public void Configure(EntityTypeBuilder<ColRepaymentRequest> builder)
    {
        builder.ToTable("COL_REPAYMENT_REQUESTS");
        builder.HasKey(x => x.RepaymentRequestId);

        builder.Property(x => x.RepaymentRequestId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ChequesAmount).HasPrecision(18, 2);
        builder.Property(x => x.RemainingAmount).HasPrecision(18, 2);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Source).HasMaxLength(30);
        builder.Property(x => x.BankTransfer).HasPrecision(28, 2);
        builder.Property(x => x.CreditBalance).HasPrecision(28, 2);

        builder.ConfigureAudit();
    }
}
