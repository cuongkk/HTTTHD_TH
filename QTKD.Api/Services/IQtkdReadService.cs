using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IQtkdReadService
{
    Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<ContractResponse?> GetContractAsync(int id, CancellationToken cancellationToken);
}
