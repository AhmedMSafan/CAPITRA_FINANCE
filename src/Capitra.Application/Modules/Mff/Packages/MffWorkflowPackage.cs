using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_WORKFLOW_PKG: application status changes along the active transitions (MFF_LKP_STATUS_TRANSITIONS), with history.</summary>
public sealed class MffWorkflowPackage(ICapitraDbContext db, ICurrentUser user, IClock clock)
{
    /// <summary>CAN_TRANSITION: an active transition from -> to for the role and action (a transition without one allows any).</summary>
    public Task<bool> CanTransitionAsync(string? fromStatus, string toStatus, string? roleCode, string? actionCode, CancellationToken ct) =>
        db.Set<MffLkpStatusTransition>().AsNoTracking().AnyAsync(t => t.FromStatusCode == fromStatus && t.ToStatusCode == toStatus && t.IsActive
            && (t.RoleCode == null || t.RoleCode == roleCode)
            && (t.ActionCode == null || t.ActionCode == actionCode), ct);

    /// <summary>CHANGE_STATUS: moves the application to the status when the transition is allowed and records it in MFF_APPLICATION_STATUS_HIST.</summary>
    public async Task ChangeStatusAsync(long applicationId, string toStatus, string? roleCode = null, string? actionCode = null,
        string? remarksAr = null, string? changedBy = null, CancellationToken ct = default)
    {
        var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new ApexProcessException("ORA-20002: طلب التمويل غير موجود.");
        var from = a.StatusCode;
        if (from == toStatus)
            return;
        if (!await CanTransitionAsync(from, toStatus, roleCode, actionCode, ct))
            throw new ApexProcessException("ORA-20001: انتقال الحالة غير مسموح: " + from + " -> " + toStatus);

        var by = changedBy ?? user.UserName ?? string.Empty;
        a.StatusCode = toStatus;
        db.Add(new MffApplicationStatusHist
        {
            ApplicationId = applicationId,
            OldStatusCode = from,
            NewStatusCode = toStatus,
            ChangedAt = clock.UtcNow,
            ChangedBy = by,
            RoleCode = roleCode,
            RemarksAr = remarksAr
        });
        await db.SaveChangesAsync(ct);
    }

    // NEW (not in Oracle): MFF_WORKFLOW_PKG.assign_work and complete_assignment are called by page 29 but the package in
    // the FINOVA database does not have them. Designed in C# from the page and MFF_WORK_ASSIGNMENTS.

    /// <summary>NEW assign_work: opens an assignment for the user and makes them the application's current owner; returns its id.</summary>
    public async Task<long> AssignWorkAsync(long applicationId, string assignmentType, string assignedTo, string? assignedBy,
        DateOnly? dueDate, string? notes, CancellationToken ct)
    {
        var a = await db.Set<MffApplication>().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new ApexProcessException("ORA-20002: طلب التمويل غير موجود.");
        var w = new MffWorkAssignment
        {
            ApplicationId = applicationId,
            AssignmentType = assignmentType,
            AssignedTo = assignedTo,
            AssignedBy = assignedBy ?? user.UserName,
            AssignedAt = clock.UtcNow,
            DueDate = dueDate,
            AssignmentStatus = "OPEN",
            Notes = notes,
        };
        db.Add(w);
        a.CurrentOwnerUser = assignedTo;
        await db.SaveChangesAsync(ct);
        return w.AssignmentId;
    }

    /// <summary>NEW complete_assignment: closes an open assignment (who closed it is its UPDATED_BY); the notes are added to the assignment's notes.</summary>
    public async Task CompleteAssignmentAsync(long assignmentId, string? notes, CancellationToken ct)
    {
        var w = await db.Set<MffWorkAssignment>().FirstOrDefaultAsync(x => x.AssignmentId == assignmentId, ct)
            ?? throw new ApexProcessException("ORA-20048: Assignment not found.");
        if (w.AssignmentStatus == "COMPLETED")
            throw new ApexProcessException("ORA-20049: Assignment is already completed.");
        w.AssignmentStatus = "COMPLETED";
        w.CompletedAt = clock.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
            w.Notes = string.IsNullOrEmpty(w.Notes) ? notes : w.Notes + "\n" + notes;
        await db.SaveChangesAsync(ct);
    }
}
