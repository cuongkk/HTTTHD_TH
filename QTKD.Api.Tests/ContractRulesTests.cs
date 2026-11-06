using QTKD.Api.Models;
using QTKD.Api.Services;

namespace QTKD.Api.Tests;

public sealed class ContractRulesTests
{
    private static Contract Draft() => new()
    {
        ContractNo = "HD-2026-019", CustomerId = 1,
        StartDate = new DateOnly(2026, 11, 1), EndDate = new DateOnly(2027, 10, 31),
        Status = "Draft", Summary = "Dịch vụ bảo trì", DocumentReference = "DOC-019"
    };

    [Theory]
    [InlineData("Draft", true)]
    [InlineData("RevisionRequested", true)]
    [InlineData("PendingApproval", false)]
    [InlineData("Approved", false)]
    public void Ctr01_EditOnlyAllowedInEditableStates(string status, bool expected) =>
        Assert.Equal(expected, ContractRules.CanEdit(status));

    [Fact]
    public void Ctr02_CompleteDraftMayBeSubmitted() =>
        Assert.Empty(ContractRules.ValidateSubmission(Draft(), customerIsActive: true));

    [Theory]
    [InlineData("", "DOC-019", "Summary")]
    [InlineData("Dịch vụ bảo trì", "", "DocumentReference")]
    public void Ctr02_MissingContentIsRejected(string summary, string documentReference, string expectedField)
    {
        var contract = Draft();
        contract.Summary = summary;
        contract.DocumentReference = documentReference;
        Assert.Contains(expectedField, ContractRules.ValidateSubmission(contract, customerIsActive: true));
    }

    [Fact]
    public void Ctr02_InactiveCustomerIsRejected() =>
        Assert.Contains("CustomerId", ContractRules.ValidateSubmission(Draft(), customerIsActive: false));

    [Fact]
    public void Ctr02_InvalidDateRangeIsRejected()
    {
        var contract = Draft();
        contract.EndDate = contract.StartDate.AddDays(-1);
        Assert.Contains("EndDate", ContractRules.ValidateSubmission(contract, customerIsActive: true));
    }

    [Fact]
    public void Ctr03_SubmitFromDraftOnlyTransitionsToPendingApproval() =>
        Assert.Equal("PendingApproval", ContractRules.SubmittedStatus);

    [Fact]
    public void SecondSubmitIsRejected()
    {
        var contract = Draft();
        contract.Status = "PendingApproval";
        Assert.Contains("Status", ContractRules.ValidateSubmission(contract, customerIsActive: true));
    }
}
