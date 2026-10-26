using Capitra.Domain.Common;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

/// <summary>Account structures, segments, segment values and accounts. Lengths follow the APEX App 120 forms.</summary>
public sealed class GlCoaStructureConfiguration : IEntityTypeConfiguration<GlCoaStructure>
{
    public void Configure(EntityTypeBuilder<GlCoaStructure> builder)
    {
        builder.ToTable("GL_COA_STRUCTURES");
        builder.HasKey(x => x.CoaId);
        builder.HasIndex(x => x.CoaCode).IsUnique();

        builder.Property(x => x.CoaCode).HasMaxLength(30);
        builder.Property(x => x.CoaNameEn).HasMaxLength(240);
        builder.Property(x => x.CoaNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DelimiterChar).HasMaxLength(3);

        builder.ConfigureAudit();
    }
}

public sealed class GlCoaSegmentConfiguration : IEntityTypeConfiguration<GlCoaSegment>
{
    public void Configure(EntityTypeBuilder<GlCoaSegment> builder)
    {
        builder.ToTable("GL_COA_SEGMENTS");
        builder.HasKey(x => x.CoaSegmentId);
        builder.HasIndex(x => new { x.CoaId, x.SegmentCode }).IsUnique();
        builder.HasIndex(x => new { x.CoaId, x.SegmentNum }).IsUnique();

        builder.Property(x => x.SegmentCode).HasMaxLength(30);
        builder.Property(x => x.SegmentNameEn).HasMaxLength(150);
        builder.Property(x => x.SegmentNameAr).HasMaxLength(150);
        builder.Property(x => x.SegmentType).HasMaxLength(30);
        builder.Property(x => x.ValueFormat).HasMaxLength(20);
        builder.Property(x => x.PadChar).HasMaxLength(1);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DefaultValueCode).HasMaxLength(30);
        builder.Property(x => x.DefaultMode).HasMaxLength(10);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlCoaSegmentValueConfiguration : IEntityTypeConfiguration<GlCoaSegmentValue>
{
    public void Configure(EntityTypeBuilder<GlCoaSegmentValue> builder)
    {
        builder.ToTable("GL_COA_SEGMENT_VALUES");
        builder.HasKey(x => x.SegmentValueId);
        builder.HasIndex(x => new { x.CoaSegmentId, x.ValueCode }).IsUnique();

        builder.Property(x => x.ValueCode).HasMaxLength(30);
        builder.Property(x => x.ValueNameEn).HasMaxLength(240);
        builder.Property(x => x.ValueNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ReferenceLegalEntityCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.ReferenceBusinessLineCode).HasMaxLength(LookupEntity.CodeMaxLength);

        builder.HasOne<GlCoaSegment>().WithMany().HasForeignKey(x => x.CoaSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.ParentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpLegalEntity>().WithMany().HasForeignKey(x => x.ReferenceLegalEntityCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdmLkpBusinessLine>().WithMany().HasForeignKey(x => x.ReferenceBusinessLineCode).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlAccountConfiguration : IEntityTypeConfiguration<GlAccount>
{
    public void Configure(EntityTypeBuilder<GlAccount> builder)
    {
        builder.ToTable("GL_ACCOUNTS");
        builder.HasKey(x => x.AccountId);
        builder.HasIndex(x => new { x.CoaId, x.AccountCode }).IsUnique();

        builder.Property(x => x.AccountCode).HasMaxLength(30);
        builder.Property(x => x.AccountNameEn).HasMaxLength(240);
        builder.Property(x => x.AccountNameAr).HasMaxLength(240);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.AccountType).HasMaxLength(30);
        builder.Property(x => x.NormalBalance).HasMaxLength(2);
        builder.Property(x => x.FinancialStatementClass).HasMaxLength(30);
        builder.Property(x => x.CashFlowClass).HasMaxLength(30);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.AccountSegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
