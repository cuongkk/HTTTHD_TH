using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class QtkdReadService(IQtkdRepository repository) : IQtkdReadService
{
    public async Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var customers = await repository.GetCustomersAsync(cancellationToken);
        return customers.Select(QtkdResponses.From).ToList();
    }

    public async Task<ContractResponse?> GetContractAsync(int id, CancellationToken cancellationToken)
    {
        var contract = await repository.GetContractAsync(id, cancellationToken);
        return contract is null ? null : QtkdResponses.From(contract);
    }

    public async Task<CustomerResponse?> GetCustomerAsync(int id, CancellationToken cancellationToken)
    {
        var customer = await repository.GetCustomerAsync(id, cancellationToken);
        return customer is null ? null : QtkdResponses.From(customer);
    }

    public async Task<IReadOnlyList<PriceListResponse>> GetPriceListsAsync(CancellationToken cancellationToken)
    {
        var priceLists = await repository.GetPriceListsAsync(cancellationToken);
        return priceLists.Select(QtkdResponses.From).ToList();
    }

    public async Task<PriceListResponse?> GetPriceListAsync(int id, CancellationToken cancellationToken)
    {
        var priceList = await repository.GetPriceListAsync(id, cancellationToken);
        return priceList is null ? null : QtkdResponses.From(priceList);
    }
}
