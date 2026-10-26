using Capitra.Domain.Common;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using Capitra.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Gl;

/// <summary>Journal sources, categories, batches, journals, account combinations and balances (APEX App 120 pages 20, 50-52).</summary>
public sealed class GlJournalSourceConfiguration : IEntityTypeConfiguration<GlJournalSource>
{
    public void Configure(EntityTypeBuilder<GlJournalSource> builder)
    {
        builder.ToTable("GL_JOURNAL_SOURCES");
        builder.HasKey(x => x.SourceId);
        builder.HasIndex(x => x.SourceCode).IsUnique();

        builder.Property(x => x.SourceCode).HasMaxLength(30);
        builder.Property(x => x.SourceNameEn).HasMaxLength(200);
        builder.Property(x => x.SourceNameAr).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.ConfigureAudit();

        // The journal entry page saves every journal under MANUAL, so it must exist.
        builder.HasData(new GlJournalSource
        {
            SourceId = 1,
            SourceCode = GlCodes.SourceManual,
            SourceNameEn = "Manual",
            SourceNameAr = "يدوي",
            IsSystemSource = true,
            IsManualAllowed = true,
            IsApprovalRequired = true,
            IsReversalAllowed = true,
            Description = "Journals entered on the journal entry page.",
            CreatedBy = AuditInterceptor.SystemUser,
            CreatedAt = GlSeed.CreatedAt,
            RowVersion = 1,
        });
    }
}

internal static class GlSeed
{
    public static readonly DateTime CreatedAt = new(2026, 10, 26, 0, 0, 0, DateTimeKind.Utc);
}

public sealed class GlJournalCategoryConfiguration : IEntityTypeConfiguration<GlJournalCategory>
{
    public void Configure(EntityTypeBuilder<GlJournalCategory> builder)
    {
        builder.ToTable("GL_JOURNAL_CATEGORIES");
        builder.HasKey(x => x.CategoryId);
        builder.HasIndex(x => x.CategoryCode).IsUnique();

        builder.Property(x => x.CategoryCode).HasMaxLength(30);
        builder.Property(x => x.CategoryNameEn).HasMaxLength(200);
        builder.Property(x => x.CategoryNameAr).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.ConfigureAudit();

        // STANDARD is the default category of a new journal (APEX J50_LOAD).
        builder.HasData(
            Seed(1, GlCodes.CategoryStandard, "Standard", "قيد عادي"),
            Seed(2, "ADJUSTMENT", "Adjustment", "قيد تسوية"));
    }

    private static GlJournalCategory Seed(long id, string code, string nameEn, string nameAr) => new()
    {
        CategoryId = id,
        CategoryCode = code,
        CategoryNameEn = nameEn,
        CategoryNameAr = nameAr,
        CreatedBy = AuditInterceptor.SystemUser,
        CreatedAt = GlSeed.CreatedAt,
        RowVersion = 1,
    };
}

public sealed class GlJournalBatchConfiguration : IEntityTypeConfiguration<GlJournalBatch>
{
    public void Configure(EntityTypeBuilder<GlJournalBatch> builder)
    {
        builder.ToTable("GL_JOURNAL_BATCHES");
        builder.HasKey(x => x.BatchId);
        builder.HasIndex(x => x.BatchNo).IsUnique();
        builder.HasIndex(x => x.SourceBatchRef);

        builder.Property(x => x.BatchNo).HasMaxLength(60);
        builder.Property(x => x.BatchName).HasMaxLength(240);
        builder.Property(x => x.SourceBatchRef).HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlJournalHeaderConfiguration : IEntityTypeConfiguration<GlJournalHeader>
{
    public void Configure(EntityTypeBuilder<GlJournalHeader> builder)
    {
        builder.ToTable("GL_JOURNAL_HEADERS");
        builder.HasKey(x => x.JournalHeaderId);
        builder.HasIndex(x => x.JournalNo).IsUnique();
        builder.HasIndex(x => new { x.LedgerId, x.AccountingDate });
        builder.HasIndex(x => x.JournalStatus);
        builder.HasIndex(x => x.ReversedHeaderId);

        builder.Property(x => x.JournalNo).HasMaxLength(60);
        builder.Property(x => x.JournalName).HasMaxLength(240);
        builder.Property(x => x.CurrencyCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.JournalStatus).HasMaxLength(30);
        builder.Property(x => x.TotalDr).HasPrecision(18, 2);
        builder.Property(x => x.TotalCr).HasPrecision(18, 2);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.ReferenceNo).HasMaxLength(120);
        builder.Property(x => x.SourceDocumentType).HasMaxLength(60);
        builder.Property(x => x.SourceDocumentNo).HasMaxLength(120);
        builder.Property(x => x.SubmittedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.ApprovedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.PostedBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.CancelledBy).HasMaxLength(AuditableConfiguration.UserNameMaxLength);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);
        builder.Property(x => x.RejectReason).HasMaxLength(1000);
        builder.Property(x => x.LastPostError).HasMaxLength(2000);

        builder.HasOne<GlJournalBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalSource>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.ReversedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlJournalLineConfiguration : IEntityTypeConfiguration<GlJournalLine>
{
    public void Configure(EntityTypeBuilder<GlJournalLine> builder)
    {
        builder.ToTable("GL_JOURNAL_LINES");
        builder.HasKey(x => x.JournalLineId);
        builder.HasIndex(x => new { x.JournalHeaderId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.AccountCombinationId);

        builder.Property(x => x.CurrencyCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.EnteredDr).HasPrecision(18, 2);
        builder.Property(x => x.EnteredCr).HasPrecision(18, 2);
        builder.Property(x => x.AccountedDr).HasPrecision(18, 2);
        builder.Property(x => x.AccountedCr).HasPrecision(18, 2);
        builder.Property(x => x.LineDescription).HasMaxLength(1000);
        builder.Property(x => x.ReferenceNo).HasMaxLength(120);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlJournalHeaderHistoryConfiguration : IEntityTypeConfiguration<GlJournalHeaderHistory>
{
    public void Configure(EntityTypeBuilder<GlJournalHeaderHistory> builder)
    {
        builder.ToTable("GL_JOURNAL_HEADERS_HIST");
        builder.HasKey(x => x.JournalHistoryId);
        builder.HasIndex(x => x.JournalHeaderId);

        builder.Property(x => x.ActionCode).HasMaxLength(30);
        builder.Property(x => x.OldStatus).HasMaxLength(30);
        builder.Property(x => x.NewStatus).HasMaxLength(30);
        builder.Property(x => x.ActionReason).HasMaxLength(2000);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlAccountCombinationConfiguration : IEntityTypeConfiguration<GlAccountCombination>
{
    public void Configure(EntityTypeBuilder<GlAccountCombination> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATIONS");
        builder.HasKey(x => x.AccountCombinationId);
        builder.HasIndex(x => new { x.CoaId, x.ConcatenatedAccount }).IsUnique();
        builder.HasIndex(x => x.AccountId);

        builder.Property(x => x.ConcatenatedAccount).HasMaxLength(400);

        builder.HasOne<GlCoaStructure>().WithMany().HasForeignKey(x => x.CoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlAccountCombinationSegmentConfiguration : IEntityTypeConfiguration<GlAccountCombinationSegment>
{
    public void Configure(EntityTypeBuilder<GlAccountCombinationSegment> builder)
    {
        builder.ToTable("GL_ACCOUNT_COMBINATION_SEGMENTS");
        builder.HasKey(x => x.CombinationSegmentId);
        builder.HasIndex(x => new { x.AccountCombinationId, x.SegmentNum }).IsUnique();
        builder.HasIndex(x => new { x.SegmentNum, x.SegmentValueCode });

        builder.Property(x => x.SegmentValueCode).HasMaxLength(30);

        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<GlCoaSegment>().WithMany().HasForeignKey(x => x.CoaSegmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlCoaSegmentValue>().WithMany().HasForeignKey(x => x.SegmentValueId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlAccountBalanceConfiguration : IEntityTypeConfiguration<GlAccountBalance>
{
    public void Configure(EntityTypeBuilder<GlAccountBalance> builder)
    {
        builder.ToTable("GL_ACCOUNT_BALANCES");
        builder.HasKey(x => x.BalanceId);
        builder.HasIndex(x => new { x.LedgerId, x.LedgerPeriodId, x.AccountCombinationId, x.CurrencyCode }).IsUnique();
        builder.HasIndex(x => x.AccountCombinationId);

        builder.Property(x => x.CurrencyCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.PeriodDr).HasPrecision(18, 2);
        builder.Property(x => x.PeriodCr).HasPrecision(18, 2);

        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.LastPostedHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class GlPostingBalanceImpactConfiguration : IEntityTypeConfiguration<GlPostingBalanceImpact>
{
    public void Configure(EntityTypeBuilder<GlPostingBalanceImpact> builder)
    {
        builder.ToTable("GL_POSTING_BALANCE_IMPACTS");
        builder.HasKey(x => x.BalanceImpactId);
        builder.HasIndex(x => x.JournalHeaderId);
        builder.HasIndex(x => new { x.AccountCombinationId, x.AccountingDate });

        builder.Property(x => x.CurrencyCode).HasMaxLength(LookupEntity.CodeMaxLength);
        builder.Property(x => x.AccountedDr).HasPrecision(18, 2);
        builder.Property(x => x.AccountedCr).HasPrecision(18, 2);
        builder.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 2);

        builder.HasOne<GlJournalHeader>().WithMany().HasForeignKey(x => x.JournalHeaderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlJournalLine>().WithMany().HasForeignKey(x => x.JournalLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedger>().WithMany().HasForeignKey(x => x.LedgerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlLedgerPeriod>().WithMany().HasForeignKey(x => x.LedgerPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GlAccountCombination>().WithMany().HasForeignKey(x => x.AccountCombinationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}
