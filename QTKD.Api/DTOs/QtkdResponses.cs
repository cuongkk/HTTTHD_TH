using QTKD.Api.Models;

namespace QTKD.Api.DTOs;

public static class QtkdResponses
{
    public static CustomerResponse From(Customer customer) =>
        new(customer.Id, customer.Name, customer.Status);

    public static ContractResponse From(Contract contract) =>
        new(contract.Id, contract.ContractNo, contract.CustomerId, contract.Customer.Name,
            contract.StartDate, contract.EndDate, contract.Status,
            contract.Summary, contract.DocumentReference);

    public static PriceItemResponse From(PriceItem item) =>
        new(item.PriceListId, item.ServiceCode, item.UnitPrice);

    public static PriceListResponse From(PriceList priceList) =>
        new(priceList.Id, priceList.Name, priceList.EffectiveDate,
            priceList.PriceItems.OrderBy(x => x.ServiceCode).Select(From).ToList());
}
