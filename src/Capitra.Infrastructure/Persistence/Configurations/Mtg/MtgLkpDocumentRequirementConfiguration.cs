using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mtg;

public sealed class MtgLkpDocumentRequirementConfiguration : IEntityTypeConfiguration<MtgLkpDocumentRequirement>
{
    public void Configure(EntityTypeBuilder<MtgLkpDocumentRequirement> builder)
    {
        builder.ToTable("MTG_LKP_DOCUMENT_REQUIREMENTS");
        builder.HasKey(x => x.RequirementCode);

        builder.Property(x => x.RequirementCode).HasMaxLength(50);

        builder.ConfigureAudit();
    }
}
