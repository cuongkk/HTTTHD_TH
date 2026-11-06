namespace QTKD.Api.DTOs;

public sealed record ContractResponse(
    int Id, string ContractNo, int CustomerId, string CustomerName,
    DateOnly StartDate, DateOnly EndDate, string Status,
    string? Summary, string? DocumentReference);
