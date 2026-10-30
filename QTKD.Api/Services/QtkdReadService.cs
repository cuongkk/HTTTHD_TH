using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class QtkdReadService(IQtkdRepository repository) : IQtkdReadService
{
    public async Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var customers = await repository.GetCustomersAsync(cancellationToken);
        return customers.Select(x => new CustomerResponse(x.Id, x.Name, x.Status)).ToList();
    }

    public async Task<ContractResponse?> GetContractAsync(int id, CancellationToken cancellationToken)
    {
        var contract = await repository.GetContractAsync(id, cancellationToken);
        return contract is null ? null : new ContractResponse(
            contract.Id, contract.ContractNo, contract.CustomerId, contract.Customer.Name,
            contract.StartDate, contract.EndDate, contract.Status);
    }
}
