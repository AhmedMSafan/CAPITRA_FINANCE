using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Capitra.Infrastructure.Persistence.Configurations.Wfe;

/// <summary>The C# BPMN workflow engine (from FINOVA_FINANCEING), which replaces the APEX workflow engine.</summary>
public sealed class WfeWorkflowDefinitionConfiguration : IEntityTypeConfiguration<WfeWorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WfeWorkflowDefinition> builder)
    {
        builder.ToTable("WFE_WORKFLOW_DEFINITIONS");
        builder.HasKey(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => x.WorkflowCode).IsUnique();

        builder.Property(x => x.WorkflowDefinitionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.WorkflowCode).HasMaxLength(50);
        builder.Property(x => x.WorkflowName).HasMaxLength(150);
        builder.Property(x => x.VersionNo).HasMaxLength(20);
        builder.Property(x => x.DefinitionStatus).HasMaxLength(30);
        builder.Property(x => x.BpmnXml).HasColumnType("text");
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.ConfigureAudit();
    }
}

public sealed class WfeDefinitionVersionConfiguration : IEntityTypeConfiguration<WfeDefinitionVersion>
{
    public void Configure(EntityTypeBuilder<WfeDefinitionVersion> builder)
    {
        builder.ToTable("WFE_DEFINITION_VERSIONS");
        builder.HasKey(x => x.DefinitionVersionId);
        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.VersionNo }).IsUnique();

        builder.Property(x => x.DefinitionVersionId).UseIdentityByDefaultColumn();
        builder.Property(x => x.BpmnXml).HasColumnType("text");
        builder.Property(x => x.ReleasedBy).HasMaxLength(100);

        builder.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeOutOfOfficeConfiguration : IEntityTypeConfiguration<WfeOutOfOffice>
{
    public void Configure(EntityTypeBuilder<WfeOutOfOffice> builder)
    {
        builder.ToTable("WFE_OUT_OF_OFFICE_ENTRIES");
        builder.HasKey(x => x.OutOfOfficeId);
        builder.HasIndex(x => new { x.UserName, x.IsActive });
        builder.HasIndex(x => x.DelegateTo);

        builder.Property(x => x.OutOfOfficeId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserName).HasMaxLength(100);
        builder.Property(x => x.DelegateTo).HasMaxLength(100);
        builder.Property(x => x.WorkflowCode).HasMaxLength(50);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.ConfigureAudit();
    }
}

public sealed class WfeAlertConfiguration : IEntityTypeConfiguration<WfeAlert>
{
    public void Configure(EntityTypeBuilder<WfeAlert> builder)
    {
        builder.ToTable("WFE_ALERTS");
        builder.HasKey(x => x.AlertId);
        builder.HasIndex(x => new { x.UserName, x.ReadAt });
        builder.HasIndex(x => x.InboxItemId);

        builder.Property(x => x.AlertId).UseIdentityByDefaultColumn();
        builder.Property(x => x.UserName).HasMaxLength(100);
        builder.Property(x => x.AlertType).HasMaxLength(30);
        builder.Property(x => x.Message).HasMaxLength(1000);

        builder.ConfigureAudit();
    }
}

public sealed class WfeDataSourceConfiguration : IEntityTypeConfiguration<WfeDataSource>
{
    public void Configure(EntityTypeBuilder<WfeDataSource> builder)
    {
        builder.ToTable("WFE_DATA_SOURCES");
        builder.HasKey(x => x.DataSourceId);
        builder.HasIndex(x => x.SourceCode).IsUnique();

        builder.Property(x => x.DataSourceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.SourceName).HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.DataSourceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeDataSourceItemConfiguration : IEntityTypeConfiguration<WfeDataSourceItem>
{
    public void Configure(EntityTypeBuilder<WfeDataSourceItem> builder)
    {
        builder.ToTable("WFE_DATA_SOURCE_ITEMS");
        builder.HasKey(x => x.DataSourceItemId);
        builder.HasIndex(x => new { x.DataSourceId, x.ItemValue }).IsUnique();

        builder.Property(x => x.DataSourceItemId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ItemValue).HasMaxLength(200);
        builder.Property(x => x.ItemLabel).HasMaxLength(300);

        builder.ConfigureAudit();
    }
}

public sealed class WfeFormConfiguration : IEntityTypeConfiguration<WfeForm>
{
    public void Configure(EntityTypeBuilder<WfeForm> builder)
    {
        builder.ToTable("WFE_FORMS");
        builder.HasKey(x => x.FormId);
        builder.HasIndex(x => x.FormCode).IsUnique();
        builder.HasIndex(x => x.WorkflowDefinitionId);

        builder.Property(x => x.FormId).UseIdentityByDefaultColumn();
        builder.Property(x => x.FormCode).HasMaxLength(50);
        builder.Property(x => x.FormName).HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.LayoutJson).HasColumnType("text");
        builder.Property(x => x.FieldsJson).HasColumnType("text");

        builder.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}

public sealed class WfeWorkflowInstanceConfiguration : IEntityTypeConfiguration<WfeWorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WfeWorkflowInstance> builder)
    {
        builder.ToTable("WFE_WORKFLOW_INSTANCES");
        builder.HasKey(x => x.WorkflowInstanceId);
        builder.HasIndex(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => x.BusinessReference);

        builder.Property(x => x.WorkflowInstanceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InstanceStatus).HasMaxLength(30);
        builder.Property(x => x.BusinessReference).HasMaxLength(150);
        builder.Property(x => x.CurrentElementId).HasMaxLength(150);
        builder.Property(x => x.StartedBy).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Version).WithMany().HasForeignKey(x => x.DefinitionVersionId).OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}

public sealed class WfeStepInstanceConfiguration : IEntityTypeConfiguration<WfeStepInstance>
{
    public void Configure(EntityTypeBuilder<WfeStepInstance> builder)
    {
        builder.ToTable("WFE_STEP_INSTANCES");
        builder.HasKey(x => x.StepInstanceId);
        builder.HasIndex(x => new { x.WorkflowInstanceId, x.ElementId });

        builder.Property(x => x.StepInstanceId).UseIdentityByDefaultColumn();
        builder.Property(x => x.ElementId).HasMaxLength(150);
        builder.Property(x => x.StepCode).HasMaxLength(100);
        builder.Property(x => x.StepName).HasMaxLength(250);
        builder.Property(x => x.ElementType).HasMaxLength(60);
        builder.Property(x => x.StepStatus).HasMaxLength(30);
        builder.Property(x => x.CompletedBy).HasMaxLength(100);
        builder.Property(x => x.Outcome).HasMaxLength(50);

        builder.HasOne(x => x.Instance).WithMany(x => x.Steps).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeTaskConfiguration : IEntityTypeConfiguration<WfeTask>
{
    public void Configure(EntityTypeBuilder<WfeTask> builder)
    {
        builder.ToTable("WFE_TASKS");
        builder.HasKey(x => x.TaskId);
        builder.HasIndex(x => x.TaskStatus);

        builder.Property(x => x.TaskId).UseIdentityByDefaultColumn();
        builder.Property(x => x.TaskStatus).HasMaxLength(30);
        builder.Property(x => x.AssignmentType).HasMaxLength(30);
        builder.Property(x => x.Assignee).HasMaxLength(150);
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.Route).HasMaxLength(500);
        builder.Property(x => x.RouteParameters).HasColumnType("text");
        builder.Property(x => x.AllowedActions).HasMaxLength(500);

        builder.HasOne(x => x.StepInstance).WithOne(x => x.Task).HasForeignKey<WfeTask>(x => x.StepInstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeInboxItemConfiguration : IEntityTypeConfiguration<WfeInboxItem>
{
    public void Configure(EntityTypeBuilder<WfeInboxItem> builder)
    {
        builder.ToTable("WFE_INBOX");
        builder.HasKey(x => x.InboxItemId);
        builder.HasIndex(x => x.TaskId).IsUnique();
        builder.HasIndex(x => new { x.InboxStatus, x.AssignedTo });
        builder.HasIndex(x => new { x.InboxStatus, x.AssignedRole });
        builder.HasIndex(x => x.WorkflowInstanceId);

        builder.Property(x => x.InboxItemId).UseIdentityByDefaultColumn();
        builder.Property(x => x.InboxStatus).HasMaxLength(30);
        builder.Property(x => x.AssignmentType).HasMaxLength(30);
        builder.Property(x => x.AssignedTo).HasMaxLength(150);
        builder.Property(x => x.AssignedRole).HasMaxLength(150);
        builder.Property(x => x.ClaimedBy).HasMaxLength(100);
        builder.Property(x => x.Subject).HasMaxLength(500);
        builder.Property(x => x.Route).HasMaxLength(500);
        builder.Property(x => x.RouteParameters).HasColumnType("text");
        builder.Property(x => x.AllowedActions).HasMaxLength(500);
        builder.Property(x => x.Outcome).HasMaxLength(50);
        builder.Property(x => x.EscalatedFrom).HasMaxLength(150);

        builder.HasOne(x => x.Task).WithOne(x => x.InboxItem).HasForeignKey<WfeInboxItem>(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Instance).WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.StepInstance).WithMany().HasForeignKey(x => x.StepInstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeInstanceVariableConfiguration : IEntityTypeConfiguration<WfeInstanceVariable>
{
    public void Configure(EntityTypeBuilder<WfeInstanceVariable> builder)
    {
        builder.ToTable("WFE_INSTANCE_VARIABLES");
        builder.HasKey(x => x.InstanceVariableId);
        builder.HasIndex(x => new { x.WorkflowInstanceId, x.VariableName }).IsUnique();

        builder.Property(x => x.InstanceVariableId).UseIdentityByDefaultColumn();
        builder.Property(x => x.VariableName).HasMaxLength(150);
        builder.Property(x => x.VariableType).HasMaxLength(30);
        builder.Property(x => x.VariableValue).HasColumnType("text");

        builder.HasOne(x => x.Instance).WithMany(x => x.Variables).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}

public sealed class WfeInstanceHistConfiguration : IEntityTypeConfiguration<WfeInstanceHist>
{
    public void Configure(EntityTypeBuilder<WfeInstanceHist> builder)
    {
        builder.ToTable("WFE_INSTANCE_HIST");
        builder.HasKey(x => x.InstanceHistId);
        builder.HasIndex(x => x.WorkflowInstanceId);

        builder.Property(x => x.InstanceHistId).UseIdentityByDefaultColumn();
        builder.Property(x => x.EventType).HasMaxLength(50);
        builder.Property(x => x.ElementId).HasMaxLength(150);
        builder.Property(x => x.ActionCode).HasMaxLength(50);
        builder.Property(x => x.PerformedBy).HasMaxLength(100);
        builder.Property(x => x.Details).HasMaxLength(2000);

        builder.HasOne<WfeWorkflowInstance>().WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAudit();
    }
}
