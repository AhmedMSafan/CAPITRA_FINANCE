namespace Capitra.Application.Abstractions;

/// <summary>Server clock. All times are UTC; screens convert to Africa/Cairo for display.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
