using Capitra.Domain.Modules.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Crm;

public sealed class CrmCfSupplierApplicationConfiguration : IEntityTypeConfiguration<CrmCfSupplierApplication>
{
    public void Configure(EntityTypeBuilder<CrmCfSupplierApplication> builder)
    {
        builder.ToTable("CRM_CF_SUPPLIER_APPLICATIONS");
        builder.HasKey(x => x.ApplicationId);
        builder.HasIndex(x => x.ApplicationRef).IsUnique();
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.StatusCode);
        builder.Property(x => x.ApplicationId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ApplicationRef).HasMaxLength(30);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.EnteredBy).HasMaxLength(256);
        builder.Property(x => x.EnteredByPerson).HasMaxLength(300);
        builder.Property(x => x.ReturnReason).HasMaxLength(1000);
        builder.Property(x => x.GoodsCategoryCodes).HasMaxLength(1000);
        builder.Property(x => x.DownPaymentCollectorCode).HasMaxLength(20);
        builder.Property(x => x.RiskRatingCode).HasMaxLength(10);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasOne<CrmClient>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CrmClientRole>().WithMany().HasForeignKey(x => x.ClientRoleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class CrmCfSupplierDocumentConfiguration : IEntityTypeConfiguration<CrmCfSupplierDocument>
{
    public void Configure(EntityTypeBuilder<CrmCfSupplierDocument> builder)
    {
        builder.ToTable("CRM_CF_SUPPLIER_DOCUMENTS");
        builder.HasKey(x => x.DocumentId);
        builder.HasIndex(x => new { x.ApplicationId, x.StepCode });
        builder.Property(x => x.DocumentId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepName).HasMaxLength(200);
        builder.Property(x => x.DocumentTitle).HasMaxLength(300);
        builder.Property(x => x.DocumentTypeCode).HasMaxLength(30);
        builder.Property(x => x.FileName).HasMaxLength(260);
        builder.Property(x => x.MimeType).HasMaxLength(100);
        builder.Property(x => x.Sha256Hash).HasMaxLength(64);
        builder.HasOne<CrmCfSupplierApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class CrmCfSupplierStepNoteConfiguration : IEntityTypeConfiguration<CrmCfSupplierStepNote>
{
    public void Configure(EntityTypeBuilder<CrmCfSupplierStepNote> builder)
    {
        builder.ToTable("CRM_CF_SUPPLIER_STEP_NOTES");
        builder.HasKey(x => x.StepNoteId);
        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.StepInstanceId).IsUnique();
        builder.Property(x => x.StepNoteId).UseIdentityByDefaultColumn();
        builder.Property(x => x.StepCode).HasMaxLength(50);
        builder.Property(x => x.StepName).HasMaxLength(200);
        builder.Property(x => x.ActionCode).HasMaxLength(20);
        builder.Property(x => x.CommentText).HasMaxLength(2000);
        builder.Property(x => x.DoneBy).HasMaxLength(256);
        builder.HasOne<CrmCfSupplierApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAudit();
    }
}

public sealed class CrmCfSupplierTermChangeConfiguration : IEntityTypeConfiguration<CrmCfSupplierTermChange>
{
    public void Configure(EntityTypeBuilder<CrmCfSupplierTermChange> builder)
    {
        builder.ToTable("CRM_CF_SUPPLIER_TERM_CHANGES");
        builder.HasKey(x => x.TermChangeId);
        builder.HasIndex(x => x.ApplicationId);
        builder.Property(x => x.TermChangeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FieldName).HasMaxLength(50);
        builder.Property(x => x.OldValue).HasMaxLength(500);
        builder.Property(x => x.NewValue).HasMaxLength(500);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.HasOne<CrmCfSupplierApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}

public sealed class CrmCfSupplierChangeRequestConfiguration : IEntityTypeConfiguration<CrmCfSupplierChangeRequest>
{
    public void Configure(EntityTypeBuilder<CrmCfSupplierChangeRequest> builder)
    {
        builder.ToTable("CRM_CF_SUPPLIER_CHANGE_REQUESTS");
        builder.HasKey(x => x.ChangeRequestId);
        builder.HasIndex(x => x.ApplicationId);
        builder.Property(x => x.ChangeRequestId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TermsJson).HasMaxLength(4000);
        builder.Property(x => x.ChangesJson).HasMaxLength(4000);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.StatusCode).HasMaxLength(20);
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.DecidedBy).HasMaxLength(100);
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.HasOne<CrmCfSupplierApplication>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAudit();
    }
}
