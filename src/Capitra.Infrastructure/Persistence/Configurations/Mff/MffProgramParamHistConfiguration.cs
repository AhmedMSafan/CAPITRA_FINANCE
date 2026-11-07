using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffProgramParamHistConfiguration : IEntityTypeConfiguration<MffProgramParamHist>
{
    public void Configure(EntityTypeBuilder<MffProgramParamHist> builder)
    {
        builder.ToTable("MFF_PROGRAM_PARAM_HIST");
        builder.HasKey(x => x.ParamHistId);
        builder.HasIndex(x => x.ParamId);
        builder.HasIndex(x => x.FinancierOrgId);
        builder.HasIndex(x => x.ProgramVersionId);

        builder.Property(x => x.ParamHistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ParamCode).HasMaxLength(50);
        builder.Property(x => x.OldValue).HasMaxLength(4000);
        builder.Property(x => x.NewValue).HasMaxLength(4000);
        builder.Property(x => x.ChangedBy).HasMaxLength(100);
        builder.Property(x => x.ChangeReasonAr).HasMaxLength(1000);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProgramParam>().WithMany().HasForeignKey(x => x.ParamId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
