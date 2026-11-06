using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public sealed class QtkdRepository(QtkdDbContext db) : IQtkdRepository
{
    public Task<List<Customer>> GetCustomersAsync(CancellationToken cancellationToken) =>
        db.Customers.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<Customer?> GetCustomerAsync(int id, CancellationToken cancellationToken) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Contract?> GetContractAsync(int id, CancellationToken cancellationToken) =>
        db.Contracts.AsNoTracking()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<PriceList>> GetPriceListsAsync(CancellationToken cancellationToken) =>
        db.PriceLists.AsNoTracking().Include(x => x.PriceItems)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<PriceList?> GetPriceListAsync(int id, CancellationToken cancellationToken) =>
        db.PriceLists.AsNoTracking().Include(x => x.PriceItems)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
