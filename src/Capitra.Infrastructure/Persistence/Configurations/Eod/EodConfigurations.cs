using Capitra.Domain.Modules.Eod;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Eod;

public sealed class EodJobConfiguration : IEntityTypeConfiguration<EodJob>
{
    public void Configure(EntityTypeBuilder<EodJob> builder)
    {
        builder.ToTable("EOD_JOBS");
        builder.HasKey(x => x.JobId);
        builder.Property(x => x.JobId).UseIdentityByDefaultColumn();
        builder.Property(x => x.JobCode).HasMaxLength(50);
        builder.Property(x => x.ModuleCode).HasMaxLength(10);
        builder.Property(x => x.NameEn).HasMaxLength(150);
        builder.Property(x => x.NameAr).HasMaxLength(150);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.JobCode).IsUnique();
        builder.ConfigureAudit();
    }
}

public sealed class EodJobParameterConfiguration : IEntityTypeConfiguration<EodJobParameter>
{
    public void Configure(EntityTypeBuilder<EodJobParameter> builder)
    {
        builder.ToTable("EOD_JOB_PARAMETERS");
        builder.HasKey(x => x.JobParameterId);
        builder.Property(x => x.JobParameterId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ParameterCode).HasMaxLength(50);
        builder.Property(x => x.ParameterValue).HasMaxLength(500);
        builder.HasIndex(x => new { x.JobId, x.ParameterCode }).IsUnique();
        builder.HasOne<EodJob>().WithMany().HasForeignKey(x => x.JobId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class EodDayConfiguration : IEntityTypeConfiguration<EodDay>
{
    public void Configure(EntityTypeBuilder<EodDay> builder)
    {
        builder.ToTable("EOD_DAYS");
        builder.HasKey(x => x.DayId);
        builder.Property(x => x.DayId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.RunBy).HasMaxLength(256);
        builder.HasIndex(x => x.BusinessDate).IsUnique();
        builder.HasMany(x => x.Jobs).WithOne().HasForeignKey(x => x.DayId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class EodDayJobConfiguration : IEntityTypeConfiguration<EodDayJob>
{
    public void Configure(EntityTypeBuilder<EodDayJob> builder)
    {
        builder.ToTable("EOD_DAY_JOBS");
        builder.HasKey(x => x.DayJobId);
        builder.Property(x => x.DayJobId).UseIdentityByDefaultColumn();
        builder.Property(x => x.JobCode).HasMaxLength(50);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.RunBy).HasMaxLength(256);
        builder.HasIndex(x => new { x.DayId, x.JobCode }).IsUnique();
        builder.ConfigureAudit();
    }
}
