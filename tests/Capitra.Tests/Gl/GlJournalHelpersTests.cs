using Capitra.Application.Modules.Gl.Journals;
using Xunit;

namespace Capitra.Tests.Gl;

/// <summary>GL helpers: transaction set numbers and the messages shown to users.</summary>
public sealed class GlJournalHelpersTests
{
    [Fact]
    public void Transaction_set_numbers_have_at_least_eight_digits()
    {
        Assert.Equal("CAP-LND-2026-00000042", GlTranSetIds.Format("LND", 2026, 42, 6));
        Assert.Equal("CAP-TRS-2026-0000000042", GlTranSetIds.Format("TRS", 2026, 42, 10));
    }

    [Fact]
    public void Messages_lose_their_ORA_prefixes_and_keep_the_first_line()
    {
        Assert.Equal("Journal J1 is not balanced.", GlPostingApi.CleanMessage("ORA-20001: ORA-06512: Journal J1 is not balanced.\nat line 4"));
        Assert.Equal(string.Empty, GlPostingApi.CleanMessage(null));
    }
}
