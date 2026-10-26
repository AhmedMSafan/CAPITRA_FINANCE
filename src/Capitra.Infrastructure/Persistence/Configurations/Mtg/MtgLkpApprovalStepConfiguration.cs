using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpApprovalStepConfiguration : IEntityTypeConfiguration<MtgLkpApprovalStep>
{
    public void Configure(EntityTypeBuilder<MtgLkpApprovalStep> builder)
    {
        builder.ToTable("MTG_LKP_APPROVAL_STEPS");
        builder.HasKey(x => x.StepCode);

        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepNameAr).HasMaxLength(200);
        builder.Property(x => x.StepNameEn).HasMaxLength(200);

        builder.ConfigureAudit();
    }
}
