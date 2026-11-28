using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseAccrualLineConfiguration : IEntityTypeConfiguration<GlCloseAccrualLine>
{
    public void Configure(EntityTypeBuilder<GlCloseAccrualLine> builder)
    {
        builder.ToTable("GL_CLOSE_ACCRUAL_LINES");
        builder.HasKey(x => x.AccrualLineId);
        builder.HasIndex(x => new { x.AccrualRunId, x.LineStatus });

        builder.Property(x => x.AccrualLineId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DrCrFlag).HasMaxLength(2);
        builder.Property(x => x.EnteredCurrencyCode).HasMaxLength(3);
        builder.Property(x => x.EnteredAmount).HasPrecision(18, 2);
        builder.Property(x => x.AccountedAmount).HasPrecision(18, 2);
        builder.Property(x => x.SourceModule).HasMaxLength(50);
        builder.Property(x => x.SourceReferenceType).HasMaxLength(50);
        builder.Property(x => x.SourceReferenceNo).HasMaxLength(100);
        builder.Property(x => x.AmountSourceCode).HasMaxLength(100);
        builder.Property(x => x.AmountSourceValue).HasPrecision(28, 2);
        builder.Property(x => x.LineStatus).HasMaxLength(30);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);

        builder.HasOne<GlCloseAccrualRun>().WithMany().HasForeignKey(x => x.AccrualRunId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCloseAccrualRuleLine>().WithMany().HasForeignKey(x => x.AccrualRuleLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
