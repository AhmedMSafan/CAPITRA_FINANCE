using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Wfe;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Tests.Workflow;

/// <summary>Who gets a task for a user who is out of office: the workflow's rule first, then the rule for all, following chains without loops.</summary>
public sealed class WorkflowOutOfOfficeTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);

    private static CapitraDbContext Db(params WfeOutOfOffice[] rows)
    {
        var db = new CapitraDbContext(new DbContextOptionsBuilder<CapitraDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        foreach (var r in rows)
        {
            r.CreatedBy = "test";
            db.Add(r);
        }

        db.SaveChanges();
        return db;
    }

    private static WfeOutOfOffice Away(string user, string to, string? workflow = null, int from = -1, int until = 3, bool on = true) =>
        new() { UserName = user, DelegateTo = to, WorkflowCode = workflow, FromDate = Today.AddDays(from), ToDate = Today.AddDays(until), IsActive = on };

    [Fact]
    public async Task A_user_in_the_office_keeps_their_tasks()
    {
        await using var db = Db(Away("ahmed", "mona", from: 1), Away("ahmed", "sara", until: -1), Away("ahmed", "omar", on: false));
        Assert.Null(await WfeOutOfOfficeService.DelegateOfAsync(db, "ahmed", "WF", Today, default));
    }

    [Fact]
    public async Task The_workflow_rule_wins_over_the_rule_for_all()
    {
        await using var db = Db(Away("ahmed", "mona"), Away("ahmed", "sara", "TREASURY"));
        Assert.Equal("sara", await WfeOutOfOfficeService.DelegateOfAsync(db, "ahmed", "TREASURY", Today, default));
        Assert.Equal("mona", await WfeOutOfOfficeService.DelegateOfAsync(db, "ahmed", "OTHER", Today, default));
    }

    [Fact]
    public async Task A_delegate_who_is_away_too_passes_it_on_without_looping()
    {
        await using var db = Db(Away("ahmed", "mona"), Away("mona", "sara"), Away("sara", "ahmed"));
        Assert.Equal("sara", await WfeOutOfOfficeService.DelegateOfAsync(db, "ahmed", "WF", Today, default));
    }
}
