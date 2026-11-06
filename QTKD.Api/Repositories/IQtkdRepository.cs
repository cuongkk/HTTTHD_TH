using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IQtkdRepository
{
    Task<List<Customer>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<Customer?> GetCustomerAsync(int id, CancellationToken cancellationToken);
    Task<Contract?> GetContractAsync(int id, CancellationToken cancellationToken);
    Task<List<PriceList>> GetPriceListsAsync(CancellationToken cancellationToken);
    Task<PriceList?> GetPriceListAsync(int id, CancellationToken cancellationToken);
}
