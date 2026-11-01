using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Fct;

public sealed class FctDisbursementChargeSourceConfiguration : IEntityTypeConfiguration<FctDisbursementChargeSource>
{
    public void Configure(EntityTypeBuilder<FctDisbursementChargeSource> builder)
    {
        builder.ToTable("FCT_DISBURSEMENT_CHARGE_SOURCES");
        builder.HasKey(x => x.DisbursementChargeSourceId);
        builder.HasIndex(x => new { x.DisbursementId, x.RepaymentSourceId }).IsUnique();

        builder.Property(x => x.DisbursementChargeSourceId).UseIdentityByDefaultColumn();

        builder.HasOne<FctDisbursement>().WithMany().HasForeignKey(x => x.DisbursementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FctRepaymentSource>().WithMany().HasForeignKey(x => x.RepaymentSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
