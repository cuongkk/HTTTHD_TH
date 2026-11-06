using QTKD.Api.Models;

namespace QTKD.Api.Services;

public static class ContractRules
{
    public const string SubmittedStatus = "PendingApproval";

    public static bool CanEdit(string status) =>
        status is "Draft" or "RevisionRequested";

    public static IReadOnlyList<string> ValidateSubmission(Contract contract, bool customerIsActive)
    {
        var errors = new List<string>();
        if (contract.Status != "Draft") errors.Add("Status");
        if (!customerIsActive) errors.Add("CustomerId");
        if (contract.StartDate == default) errors.Add("StartDate");
        if (contract.EndDate == default || contract.EndDate < contract.StartDate) errors.Add("EndDate");
        if (string.IsNullOrWhiteSpace(contract.Summary)) errors.Add("Summary");
        if (string.IsNullOrWhiteSpace(contract.DocumentReference)) errors.Add("DocumentReference");
        return errors;
    }
}
