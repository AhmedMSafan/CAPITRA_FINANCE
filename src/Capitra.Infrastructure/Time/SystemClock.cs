using Capitra.Application.Abstractions;

namespace Capitra.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
