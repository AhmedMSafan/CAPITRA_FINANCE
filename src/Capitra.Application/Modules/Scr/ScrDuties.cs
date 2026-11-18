using Capitra.Application.Common.Exceptions;

namespace Capitra.Application.Modules.Scr;

/// <summary>
/// Segregation of duties on a decision, by person and not by account: the FINOVA prototype compared accounts, so one person with
/// a reviewer account and a publisher account could review and publish the same decision. A person is the user's employee number,
/// or the user name when there is none.
/// </summary>
public static class ScrDuties
{
    public static string PersonKey(string? employeeNo, string userName) =>
        string.IsNullOrWhiteSpace(employeeNo) ? "USER:" + userName.Trim().ToUpperInvariant() : "EMP:" + employeeNo.Trim().ToUpperInvariant();

    public static void CheckReview(string enteredByPerson, string reviewerPerson)
    {
        if (enteredByPerson == reviewerPerson)
            throw new BusinessRuleException("Segregation of duties: the person who entered a decision cannot review it.");
    }

    public static void CheckApproval(string enteredByPerson, string? reviewedByPerson, string approverPerson)
    {
        if (approverPerson == enteredByPerson || approverPerson == reviewedByPerson)
            throw new BusinessRuleException("Segregation of duties: the person who entered or reviewed a decision cannot approve it.");
    }
}
