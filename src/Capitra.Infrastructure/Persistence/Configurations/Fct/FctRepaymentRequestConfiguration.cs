using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctRepaymentRequestConfiguration : IEntityTypeConfiguration<FctRepaymentRequest>
{
    public void Configure(EntityTypeBuilder<FctRepaymentRequest> builder)
    {
        builder.ToTable("FCT_REPAYMENT_REQUESTS");
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
