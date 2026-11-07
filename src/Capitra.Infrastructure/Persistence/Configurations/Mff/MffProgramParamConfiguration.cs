using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProgramParamConfiguration : IEntityTypeConfiguration<MffProgramParam>
{
    public void Configure(EntityTypeBuilder<MffProgramParam> builder)
    {
        builder.ToTable("MFF_PROGRAM_PARAMS");
        builder.HasKey(x => x.ParamId);
        builder.HasIndex(x => new { x.ProgramVersionId, x.FinancierId, x.ParamCode, x.EffectiveFromDate }).IsUnique();
        builder.HasIndex(x => x.FinancierId);
        builder.HasIndex(x => x.ProgramVersionId);

        builder.Property(x => x.ParamId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ParamCode).HasMaxLength(100);
        builder.Property(x => x.ParamNameAr).HasMaxLength(250);
        builder.Property(x => x.ValueNumber).HasPrecision(24, 6);
        builder.Property(x => x.ValueText).HasMaxLength(1000);
        builder.Property(x => x.UnitCode).HasMaxLength(30);
        builder.Property(x => x.SourceReference).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
