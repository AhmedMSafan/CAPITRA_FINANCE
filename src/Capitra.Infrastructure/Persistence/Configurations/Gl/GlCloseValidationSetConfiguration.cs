using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlCloseValidationSetConfiguration : IEntityTypeConfiguration<GlCloseValidationSet>
{
    public void Configure(EntityTypeBuilder<GlCloseValidationSet> builder)
    {
        builder.ToTable("GL_CLOSE_VALIDATION_SETS");
        builder.HasKey(x => x.ValidationSetId);
        builder.HasIndex(x => x.ValidationSetCode).IsUnique();

        builder.Property(x => x.ValidationSetId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ValidationSetCode).HasMaxLength(50);
        builder.Property(x => x.ValidationSetNameEn).HasMaxLength(200);
        builder.Property(x => x.ValidationSetNameAr).HasMaxLength(200);
        builder.Property(x => x.CloseType).HasMaxLength(20);
        builder.Property(x => x.SeverityOnFailure).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
