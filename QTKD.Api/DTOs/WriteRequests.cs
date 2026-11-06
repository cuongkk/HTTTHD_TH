namespace QTKD.Api.DTOs;

public sealed record CreateCustomerRequest(string? Name);
public sealed record UpdateCustomerRequest(string? Name, string? Status);

public sealed record CreateContractRequest(
    string? ContractNo, int CustomerId, DateOnly? StartDate, DateOnly? EndDate,
    string? Summary, string? DocumentReference, string? Status = null);

public sealed record UpdateContractRequest(
    string? ContractNo, int CustomerId, DateOnly? StartDate, DateOnly? EndDate,
    string? Summary, string? DocumentReference, string? Status = null);

public sealed record CreatePriceListRequest(string? Name, DateOnly? EffectiveDate);
public sealed record AddPriceItemRequest(string? ServiceCode, decimal UnitPrice);
