using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

public sealed class GlJournalSourceConfiguration : IEntityTypeConfiguration<GlJournalSource>
{
    public void Configure(EntityTypeBuilder<GlJournalSource> builder)
    {
        builder.ToTable("GL_JOURNAL_SOURCES");
        builder.HasKey(x => x.SourceId);
        builder.HasIndex(x => x.SourceCode).IsUnique();

        builder.Property(x => x.SourceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.SourceNameEn).HasMaxLength(200);
        builder.Property(x => x.SourceNameAr).HasMaxLength(200);
        builder.Property(x => x.SourceGroupCode).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}
