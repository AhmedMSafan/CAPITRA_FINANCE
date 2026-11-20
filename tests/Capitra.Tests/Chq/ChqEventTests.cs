using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Chq.Packages;
using Xunit;

namespace Capitra.Tests.Chq;

/// <summary>The cheque life cycle (chq_event_pkg): the status each event gives, and which events each status allows.</summary>
public sealed class ChqEventTests
{
    [Theory]
    [InlineData("RECEIVED", "NEW", "RECEIVED")]
    [InlineData("deposited", "RECEIVED", "DEPOSITED")]
    [InlineData("REDEPOSITED", "REJECTED", "DEPOSITED")]
    [InlineData("COLLECTED", "DEPOSITED", "COLLECTED")]
    [InlineData("REPLACEMENT_ISSUED", "REJECTED", "REPLACED")]
    [InlineData("SETTLED_INTERNAL", "COLLECTED", "SETTLED")]
    [InlineData("LOCATION_TRANSFER", "DEPOSITED", "DEPOSITED")]
    public void Each_event_gives_its_status(string eventType, string old, string expected) =>
        Assert.Equal(expected, ChqEventPackage.DeriveStatus(eventType, old));

    [Theory]
    [InlineData("RECEIVED", "NEW")]
    [InlineData("DEPOSITED", "RECEIVED")]
    [InlineData("DEPOSITED", "WITHDRAWN")]
    [InlineData("COLLECTED", "DEPOSITED")]
    [InlineData("REJECTED", "DEPOSITED")]
    [InlineData("REDEPOSITED", "REJECTED")]
    [InlineData("REPLACEMENT_ISSUED", "REJECTED")]
    [InlineData("SETTLED_TRANSFER", "COLLECTED")]
    [InlineData("LOCATION_TRANSFER", "SETTLED")]
    public void Allowed_transitions(string eventType, string old) => ChqEventPackage.ValidateTransition(1, old, eventType);

    [Theory]
    [InlineData("DEPOSITED", "COLLECTED")]
    [InlineData("COLLECTED", "RECEIVED")]
    [InlineData("REPLACEMENT_ISSUED", "DEPOSITED")]
    [InlineData("SETTLED_TRANSFER", "DEPOSITED")]
    [InlineData("RECEIVED", "DEPOSITED")]
    [InlineData("DEPOSITED", "SETTLED")]
    [InlineData("COLLECTED", "CANCELLED")]
    [InlineData("BOUNCED", "DEPOSITED")]
    public void Refused_transitions(string eventType, string old) =>
        Assert.Throws<ApexProcessException>(() => ChqEventPackage.ValidateTransition(1, old, eventType));

    [Fact]
    public void A_cheque_without_a_status_passes_as_in_the_PLSQL()
    {
        ChqEventPackage.ValidateTransition(1, null, "COLLECTED");
        Assert.Null(ChqEventPackage.DeriveStatus("LOCATION_TRANSFER", null));
    }

    [Fact]
    public void A_rejection_needs_its_reason()
    {
        Assert.Throws<ApexProcessException>(() => ChqEventPackage.ValidateEventData("REJECTED", null));
        ChqEventPackage.ValidateEventData("REJECTED", "INSUFFICIENT_FUNDS");
        ChqEventPackage.ValidateEventData("DEPOSITED", null);
    }
}
