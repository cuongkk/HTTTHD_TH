using Microsoft.EntityFrameworkCore;
using Npgsql;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Models;

namespace QTKD.Api.Services;

public sealed class QtkdWriteService(QtkdDbContext db) : IQtkdWriteService
{
    public async Task<ServiceResult<CustomerResponse>> CreateCustomerAsync(
        CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ValidText(request.Name, 200))
            return ServiceResult<CustomerResponse>.BadRequest("InvalidName", "Tên Customer bắt buộc và tối đa 200 ký tự.");

        var customer = new Customer { Name = request.Name!.Trim(), Status = "Active" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult<CustomerResponse>.Created(QtkdResponses.From(customer));
    }

    public async Task<ServiceResult<CustomerResponse>> UpdateCustomerAsync(
        int id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.FindAsync([id], cancellationToken);
        if (customer is null) return ServiceResult<CustomerResponse>.NotFound("Không tìm thấy Customer.");
        if (!ValidText(request.Name, 200))
            return ServiceResult<CustomerResponse>.BadRequest("InvalidName", "Tên Customer bắt buộc và tối đa 200 ký tự.");
        if (request.Status is not ("Active" or "Inactive"))
            return ServiceResult<CustomerResponse>.BadRequest("InvalidStatus", "Status phải là Active hoặc Inactive.");

        customer.Name = request.Name!.Trim();
        customer.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult<CustomerResponse>.Ok(QtkdResponses.From(customer));
    }

    public async Task<ServiceResult<ContractResponse>> CreateContractAsync(
        CreateContractRequest request, CancellationToken cancellationToken)
    {
        if (request.Status is not null)
            return ServiceResult<ContractResponse>.BadRequest("CTR-03", "Không được đặt Status trực tiếp; Contract mới luôn là Draft.");
        var error = ValidateContractInput(request.ContractNo, request.StartDate, request.EndDate,
            request.Summary, request.DocumentReference);
        if (error is not null) return ServiceResult<ContractResponse>.BadRequest("InvalidContract", error);

        var customer = await db.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer?.Status != "Active")
            return ServiceResult<ContractResponse>.BadRequest("InvalidCustomer", "Customer phải tồn tại và đang Active.");
        var contractNo = request.ContractNo!.Trim();
        if (await db.Contracts.AnyAsync(x => x.ContractNo == contractNo, cancellationToken))
            return ServiceResult<ContractResponse>.Conflict("DuplicateContractNo", "ContractNo đã tồn tại.");

        var contract = new Contract
        {
            ContractNo = contractNo,
            CustomerId = customer.Id,
            Customer = customer,
            StartDate = request.StartDate!.Value,
            EndDate = request.EndDate!.Value,
            Summary = TrimOrNull(request.Summary),
            DocumentReference = TrimOrNull(request.DocumentReference),
            Status = "Draft"
        };
        db.Contracts.Add(contract);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (IsIntegrityConflict(ex))
        {
            return ServiceResult<ContractResponse>.Conflict("WriteConflict", "ContractNo hoặc Customer không còn hợp lệ.");
        }
        return ServiceResult<ContractResponse>.Created(QtkdResponses.From(contract));
    }

    public async Task<ServiceResult<ContractResponse>> UpdateContractAsync(
        int id, UpdateContractRequest request, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts.Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (contract is null) return ServiceResult<ContractResponse>.NotFound("Không tìm thấy Contract.");
        if (!ContractRules.CanEdit(contract.Status))
            return ServiceResult<ContractResponse>.Conflict("CTR-01", "Chỉ được sửa Contract ở Draft hoặc RevisionRequested.");
        if (request.Status is not null)
            return ServiceResult<ContractResponse>.BadRequest("CTR-03", "Không được đặt Status trực tiếp; dùng endpoint submit.");

        var error = ValidateContractInput(request.ContractNo, request.StartDate, request.EndDate,
            request.Summary, request.DocumentReference);
        if (error is not null) return ServiceResult<ContractResponse>.BadRequest("InvalidContract", error);
        var customer = await db.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer?.Status != "Active")
            return ServiceResult<ContractResponse>.BadRequest("InvalidCustomer", "Customer phải tồn tại và đang Active.");
        var contractNo = request.ContractNo!.Trim();
        if (await db.Contracts.AnyAsync(x => x.Id != id && x.ContractNo == contractNo, cancellationToken))
            return ServiceResult<ContractResponse>.Conflict("DuplicateContractNo", "ContractNo đã tồn tại.");

        contract.ContractNo = contractNo;
        contract.CustomerId = customer.Id;
        contract.Customer = customer;
        contract.StartDate = request.StartDate!.Value;
        contract.EndDate = request.EndDate!.Value;
        contract.Summary = TrimOrNull(request.Summary);
        contract.DocumentReference = TrimOrNull(request.DocumentReference);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<ContractResponse>.Conflict("CTR-01", "Trạng thái Contract đã thay đổi; tải lại trước khi sửa.");
        }
        catch (DbUpdateException ex) when (IsIntegrityConflict(ex))
        {
            return ServiceResult<ContractResponse>.Conflict("WriteConflict", "ContractNo hoặc Customer không còn hợp lệ.");
        }
        return ServiceResult<ContractResponse>.Ok(QtkdResponses.From(contract));
    }

    public async Task<ServiceResult<ContractResponse>> SubmitContractAsync(int id, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts.Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (contract is null) return ServiceResult<ContractResponse>.NotFound("Không tìm thấy Contract.");
        if (contract.Status != "Draft")
            return ServiceResult<ContractResponse>.Conflict("InvalidTransition", "Chỉ được gửi Contract ở Draft; gửi lặp bị từ chối.");
        var missing = ContractRules.ValidateSubmission(contract, contract.Customer?.Status == "Active");
        if (missing.Count > 0)
            return ServiceResult<ContractResponse>.BadRequest("CTR-02", "Không thể gửi Contract. Kiểm tra: " + string.Join(", ", missing) + ".");

        contract.Status = ContractRules.SubmittedStatus;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<ContractResponse>.Conflict("InvalidTransition", "Contract đã được sửa hoặc gửi bởi yêu cầu khác.");
        }
        return ServiceResult<ContractResponse>.Ok(QtkdResponses.From(contract));
    }

    public async Task<ServiceResult<PriceListResponse>> CreatePriceListAsync(
        CreatePriceListRequest request, CancellationToken cancellationToken)
    {
        if (!ValidText(request.Name, 200) || request.EffectiveDate is null or { Year: 1 })
            return ServiceResult<PriceListResponse>.BadRequest("InvalidPriceList", "Tên và EffectiveDate hợp lệ là bắt buộc.");
        var priceList = new PriceList { Name = request.Name!.Trim(), EffectiveDate = request.EffectiveDate.Value };
        db.PriceLists.Add(priceList);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult<PriceListResponse>.Created(QtkdResponses.From(priceList));
    }

    public async Task<ServiceResult<PriceItemResponse>> AddPriceItemAsync(
        int priceListId, AddPriceItemRequest request, CancellationToken cancellationToken)
    {
        var priceList = await db.PriceLists.FindAsync([priceListId], cancellationToken);
        if (priceList is null) return ServiceResult<PriceItemResponse>.NotFound("Không tìm thấy PriceList.");
        if (!ValidText(request.ServiceCode, 50) || request.UnitPrice <= 0 ||
            decimal.Round(request.UnitPrice, 2) != request.UnitPrice || request.UnitPrice > 9999999999999999.99m)
            return ServiceResult<PriceItemResponse>.BadRequest("InvalidPriceItem", "ServiceCode bắt buộc; UnitPrice phải dương và có tối đa 2 chữ số thập phân.");
        var code = request.ServiceCode!.Trim();
        if (await db.PriceItems.AnyAsync(x => x.PriceListId == priceListId && x.ServiceCode == code, cancellationToken))
            return ServiceResult<PriceItemResponse>.Conflict("DuplicatePriceItem", "ServiceCode đã có trong PriceList.");
        var item = new PriceItem { PriceListId = priceListId, PriceList = priceList, ServiceCode = code, UnitPrice = request.UnitPrice };
        db.PriceItems.Add(item);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (IsIntegrityConflict(ex))
        {
            return ServiceResult<PriceItemResponse>.Conflict("WriteConflict", "PriceItem đã tồn tại hoặc PriceList không còn hợp lệ.");
        }
        return ServiceResult<PriceItemResponse>.Created(QtkdResponses.From(item));
    }

    private static bool ValidText(string? value, int maxLength) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= maxLength;

    private static bool IsIntegrityConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException pg &&
        pg.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation;

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? ValidateContractInput(
        string? contractNo, DateOnly? startDate, DateOnly? endDate,
        string? summary, string? documentReference)
    {
        if (!ValidText(contractNo, 50)) return "ContractNo bắt buộc và tối đa 50 ký tự.";
        if (startDate is null or { Year: 1 }) return "StartDate bắt buộc.";
        if (endDate is null or { Year: 1 } || endDate < startDate) return "EndDate phải từ StartDate trở đi.";
        if (summary?.Trim().Length > 2000) return "Summary tối đa 2000 ký tự.";
        if (documentReference?.Trim().Length > 500) return "DocumentReference tối đa 500 ký tự.";
        return null;
    }
}
