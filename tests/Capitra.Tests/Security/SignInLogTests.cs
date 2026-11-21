using Capitra.Infrastructure.Identity;
using Xunit;

namespace Capitra.Tests.Security;

public sealed class SignInLogTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36", "Chrome · Windows")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0", "Edge · Windows")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1", "Safari · iOS")]
    [InlineData("curl/8.5.0", "curl")]
    [InlineData(null, null)]
    public void The_device_is_read_from_the_browser(string? agent, string? device) => Assert.Equal(device, SignInLog.Device(agent));
}
