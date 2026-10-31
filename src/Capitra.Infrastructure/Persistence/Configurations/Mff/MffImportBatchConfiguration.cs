using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Mff;

public sealed class MffImportBatchConfiguration : IEntityTypeConfiguration<MffImportBatch>
{
    public void Configure(EntityTypeBuilder<MffImportBatch> builder)
    {
        builder.ToTable("MFF_IMPORT_BATCHES");
        builder.HasKey(x => x.BatchId);
        builder.HasIndex(x => x.FinancierId);
        builder.HasIndex(x => x.FundOrgId);
        builder.HasIndex(x => x.ProgramVersionId);
        builder.HasIndex(x => x.SourceChannelCode);

        builder.Property(x => x.BatchId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceChannelCode).HasMaxLength(50);
        builder.Property(x => x.FileName).HasMaxLength(500);
        builder.Property(x => x.BatchStatus).HasMaxLength(30);
        builder.Property(x => x.FileChecksum).HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FinancierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffOrganization>().WithMany().HasForeignKey(x => x.FundOrgId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffProgramVersion>().WithMany().HasForeignKey(x => x.ProgramVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MffLkpSourceChannel>().WithMany().HasForeignKey(x => x.SourceChannelCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
