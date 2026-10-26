using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlAutoRuleTestCaseConfiguration : IEntityTypeConfiguration<GlAutoRuleTestCase>
{
    public void Configure(EntityTypeBuilder<GlAutoRuleTestCase> builder)
    {
        builder.ToTable("GL_AUTO_RULE_TEST_CASES");
        builder.HasKey(x => x.TestCaseId);
        builder.HasIndex(x => new { x.RuleSetId, x.TestCaseCode }).IsUnique();

        builder.Property(x => x.TestCaseId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TestCaseCode).HasMaxLength(80);
        builder.Property(x => x.TestCaseNameEn).HasMaxLength(240);
        builder.Property(x => x.ExpectedTotalDr).HasPrecision(22, 5);
        builder.Property(x => x.ExpectedTotalCr).HasPrecision(22, 5);
        builder.Property(x => x.TestStatus).HasMaxLength(30);
        builder.Property(x => x.LastRunBy).HasMaxLength(255);
        builder.Property(x => x.LastResultStatus).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlAutoAccountingRuleSet>().WithMany().HasForeignKey(x => x.RuleSetId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
