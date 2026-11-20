using Capitra.Application.Modules.Wfe;

namespace Capitra.Api.Startup;

/// <summary>Checks the workflow tasks' time limits (SLA) every minute: reminders, overdue alerts and escalations.</summary>
public sealed partial class WfeSlaWorker(IServiceScopeFactory scopes, ILogger<WfeSlaWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var result = await scope.ServiceProvider.GetRequiredService<WfeSla>().RunAsync(stoppingToken);
                if (result.Reminded + result.Overdue + result.Escalated > 0)
                    Ran(logger, result.Reminded, result.Overdue, result.Escalated);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
#pragma warning disable CA1031 // the checker must keep running; the next minute tries again
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Failed(logger, ex);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Workflow deadlines: {Reminded} reminded, {Overdue} overdue, {Escalated} escalated.")]
    private static partial void Ran(ILogger logger, int reminded, int overdue, int escalated);

    [LoggerMessage(Level = LogLevel.Error, Message = "Workflow deadline check failed.")]
    private static partial void Failed(ILogger logger, Exception ex);
}
