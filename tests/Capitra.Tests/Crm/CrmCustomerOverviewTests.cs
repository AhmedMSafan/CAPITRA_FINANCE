using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Crm;

namespace Capitra.Tests.Crm;

/// <summary>Customer 360 > Overview: what counts as open work and how a status reads (Ahmed, 26 Nov 2026).</summary>
public sealed class CrmCustomerOverviewTests
{
    [Theory]
    [InlineData("ACTIVE", true)]
    [InlineData("cancelled", true)]
    [InlineData(null, true)] // no status: nothing to follow up
    [InlineData("DRAFT", false)]
    [InlineData("APPLICATION", false)]
    public void Finished_factoring_work_is_not_open(string? status, bool finished) =>
        Assert.Equal(finished, CrmCustomerOverviewService.IsFinished(status));

    [Fact]
    public void A_supplier_application_reads_as_its_status()
    {
        Assert.Equal("In approval", CrmCustomerOverviewService.SupplierStatus(CrmCfSupplierStatus.InWorkflow));
        Assert.Equal("Returned to Sales", CrmCustomerOverviewService.SupplierStatus(CrmCfSupplierStatus.Returned));
        Assert.Equal("Draft: not submitted yet", CrmCustomerOverviewService.SupplierStatus(CrmCfSupplierStatus.Draft));
    }

    [Fact]
    public void A_code_reads_as_words() =>
        Assert.Equal("In review", CrmCustomerOverviewService.Label("IN_REVIEW"));
}
