using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlPostingValidationSetConfiguration : IEntityTypeConfiguration<GlPostingValidationSet>
{
    public void Configure(EntityTypeBuilder<GlPostingValidationSet> builder)
    {
        builder.ToTable("GL_POSTING_VALIDATION_SETS");
        builder.HasKey(x => x.ValidationSetId);
        builder.HasIndex(x => x.ValidationSetCode).IsUnique();

        builder.Property(x => x.ValidationSetId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValidationSetCode).HasMaxLength(50);
        builder.Property(x => x.ValidationSetName).HasMaxLength(200);
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.CategoryCode).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
