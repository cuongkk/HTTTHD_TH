using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IQtkdRepository
{
    Task<List<Customer>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<Contract?> GetContractAsync(int id, CancellationToken cancellationToken);
}
