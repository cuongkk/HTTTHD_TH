using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IQtkdReadService
{
    Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<CustomerResponse?> GetCustomerAsync(int id, CancellationToken cancellationToken);
    Task<ContractResponse?> GetContractAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PriceListResponse>> GetPriceListsAsync(CancellationToken cancellationToken);
    Task<PriceListResponse?> GetPriceListAsync(int id, CancellationToken cancellationToken);
}
