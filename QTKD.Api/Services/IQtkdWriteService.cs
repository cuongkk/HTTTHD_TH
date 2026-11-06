using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IQtkdWriteService
{
    Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CustomerResponse>> UpdateCustomerAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ContractResponse>> CreateContractAsync(CreateContractRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ContractResponse>> UpdateContractAsync(int id, UpdateContractRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ContractResponse>> SubmitContractAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<PriceListResponse>> CreatePriceListAsync(CreatePriceListRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<PriceItemResponse>> AddPriceItemAsync(int priceListId, AddPriceItemRequest request, CancellationToken cancellationToken);
}
