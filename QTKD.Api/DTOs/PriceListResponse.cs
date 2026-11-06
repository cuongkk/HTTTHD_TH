namespace QTKD.Api.DTOs;

public sealed record PriceItemResponse(int PriceListId, string ServiceCode, decimal UnitPrice);
public sealed record PriceListResponse(
    int Id, string Name, DateOnly EffectiveDate, IReadOnlyList<PriceItemResponse> Items);
