using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Chq;

public sealed class ChqLkpRejectionReasonConfiguration : IEntityTypeConfiguration<ChqLkpRejectionReason>
{
    public void Configure(EntityTypeBuilder<ChqLkpRejectionReason> builder)
    {
        builder.ToTable("CHQ_LKP_REJECTION_REASONS");
        builder.HasKey(x => x.ReasonCode);

        builder.Property(x => x.ReasonCode).HasMaxLength(50);
        builder.Property(x => x.NameEn).HasMaxLength(300);
        builder.Property(x => x.NameAr).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}
