using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProgramDocRequirementConfiguration : IEntityTypeConfiguration<MffProgramDocRequirement>
{
    public void Configure(EntityTypeBuilder<MffProgramDocRequirement> builder)
    {
        builder.ToTable("MFF_PROGRAM_DOC_REQUIREMENTS");
        builder.HasKey(x => x.RequirementId);
        builder.HasIndex(x => new { x.ProgramVersionId, x.DocumentTypeCode, x.PartyRoleCode, x.EmploymentTypeCode }).IsUnique();
        builder.HasIndex(x => x.DocumentTypeCode);
        builder.HasIndex(x => x.ProgramVersionId);
        builder.HasIndex(x => x.EmploymentTypeCode);
        builder.HasIndex(x => x.PartyRoleCode);

        builder.Property(x => x.RequirementId).UseIdentityByDefaultColumn();
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(40);
        builder.Property(x => x.PartyRoleCode).HasMaxLength(30);
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(30);
        builder.Property(x => x.NotesAr).HasMaxLength(1000);

        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpEmploymentType>().WithMany().HasForeignKey(x => x.EmploymentTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpDocumentType>().WithMany().HasForeignKey(x => x.DocumentTypeCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpPartyRole>().WithMany().HasForeignKey(x => x.PartyRoleCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
