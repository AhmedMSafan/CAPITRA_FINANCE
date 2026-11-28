using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoRuleTestResultConfiguration : IEntityTypeConfiguration<GlAutoRuleTestResult>
{
    public void Configure(EntityTypeBuilder<GlAutoRuleTestResult> builder)
    {
        builder.ToTable("GL_AUTO_RULE_TEST_RESULTS");
        builder.HasKey(x => x.TestResultId);
        builder.HasIndex(x => new { x.TestCaseId, x.RunNo }).IsUnique();

        builder.Property(x => x.TestResultId).UseIdentityByDefaultColumn();
        builder.Property(x => x.RunBy).HasMaxLength(255);
        builder.Property(x => x.ResultStatus).HasMaxLength(30);
        builder.Property(x => x.GeneratedTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.GeneratedTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.DifferenceAmount).HasPrecision(18, 2);
        builder.Property(x => x.ResultMessage).HasMaxLength(4000);

        builder.HasOne<GlAutoRuleTestCase>().WithMany().HasForeignKey(x => x.TestCaseId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
