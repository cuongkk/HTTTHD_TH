using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Services;

namespace QTKD.Api.Tests;

public sealed class Week06FlowTests
{
    private static QtkdDbContext Database()
    {
        var options = new DbContextOptionsBuilder<QtkdDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new QtkdDbContext(options);
    }

    [Fact]
    public async Task CustomerRequiresNameAndPersistsValidCustomer()
    {
        await using var db = Database();
        var service = new QtkdWriteService(db);
        Assert.Equal(400, (await service.CreateCustomerAsync(new(null), default)).StatusCode);
        var created = await service.CreateCustomerAsync(new("Công ty mới"), default);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal("Công ty mới", (await db.Customers.SingleAsync()).Name);
    }

    [Fact]
    public async Task CustomerUpdateRejectsInvalidStatusWithoutChangingRow()
    {
        await using var db = Database();
        db.Customers.Add(new Customer { Id = 1, Name = "Tên cũ", Status = "Active" });
        await db.SaveChangesAsync();
        var service = new QtkdWriteService(db);
        Assert.Equal(400, (await service.UpdateCustomerAsync(1, new("Tên mới", "Unknown"), default)).StatusCode);
        Assert.Equal("Tên cũ", (await db.Customers.SingleAsync()).Name);
        Assert.Equal(200, (await service.UpdateCustomerAsync(1, new("Tên mới", "Inactive"), default)).StatusCode);
        Assert.Equal("Inactive", (await db.Customers.SingleAsync()).Status);
    }

    [Fact]
    public async Task DraftNeedsContentBeforeSubmitAndSecondSubmitIsRejected()
    {
        await using var db = Database();
        db.Customers.Add(new Customer { Id = 1, Name = "Khách hàng", Status = "Active" });
        await db.SaveChangesAsync();
        var service = new QtkdWriteService(db);
        var created = await service.CreateContractAsync(
            new("HD-2026-019", 1, new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31), null, null), default);
        Assert.Equal(201, created.StatusCode);
        var id = created.Value!.Id;
        Assert.Equal(400, (await service.SubmitContractAsync(id, default)).StatusCode);
        Assert.Equal("Draft", (await db.Contracts.SingleAsync()).Status);

        var updated = await service.UpdateContractAsync(id,
            new("HD-2026-019", 1, new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
                "Dịch vụ bảo trì", "DOC-019"), default);
        Assert.Equal(200, updated.StatusCode);
        Assert.Equal("PendingApproval", (await service.SubmitContractAsync(id, default)).Value!.Status);
        Assert.Equal(409, (await service.SubmitContractAsync(id, default)).StatusCode);
        Assert.Equal("PendingApproval", (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task PendingApprovalContractCannotBeEdited()
    {
        await using var db = Database();
        db.Customers.Add(new Customer { Id = 1, Name = "Khách hàng", Status = "Active" });
        db.Contracts.Add(new Contract
        {
            Id = 1, ContractNo = "HD-2026-019", CustomerId = 1,
            StartDate = new DateOnly(2026, 11, 1), EndDate = new DateOnly(2027, 10, 31),
            Status = "PendingApproval"
        });
        await db.SaveChangesAsync();
        var service = new QtkdWriteService(db);
        var result = await service.UpdateContractAsync(1,
            new("HD-CHANGED", 1, new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31), null, null), default);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal("HD-2026-019", (await db.Contracts.SingleAsync()).ContractNo);
    }

    [Fact]
    public async Task DirectApprovedStatusIsRejected()
    {
        await using var db = Database();
        db.Customers.Add(new Customer { Id = 1, Name = "Khách hàng", Status = "Active" });
        await db.SaveChangesAsync();
        var service = new QtkdWriteService(db);
        var create = await service.CreateContractAsync(
            new("HD-2026-019", 1, new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
                "Nội dung", "DOC-019", "Approved"), default);
        Assert.Equal(400, create.StatusCode);
        Assert.Empty(await db.Contracts.ToListAsync());

        db.Contracts.Add(new Contract
        {
            Id = 1, ContractNo = "HD-2026-019", CustomerId = 1,
            StartDate = new DateOnly(2026, 11, 1), EndDate = new DateOnly(2027, 10, 31), Status = "Draft"
        });
        await db.SaveChangesAsync();
        var update = await service.UpdateContractAsync(1,
            new("HD-2026-019", 1, new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
                "Nội dung", "DOC-019", "Approved"), default);
        Assert.Equal(400, update.StatusCode);
        Assert.Equal("Draft", (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task SubmitRejectsInactiveCustomerWithoutChangingDraft()
    {
        await using var db = Database();
        db.Customers.Add(new Customer { Id = 1, Name = "Khách hàng", Status = "Inactive" });
        db.Contracts.Add(new Contract
        {
            Id = 1, ContractNo = "HD-2026-019", CustomerId = 1,
            StartDate = new DateOnly(2026, 11, 1), EndDate = new DateOnly(2027, 10, 31),
            Status = "Draft", Summary = "Nội dung", DocumentReference = "DOC-019"
        });
        await db.SaveChangesAsync();
        var result = await new QtkdWriteService(db).SubmitContractAsync(1, default);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Draft", (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task PriceItemNeedsPositivePriceAndExistingList()
    {
        await using var db = Database();
        var service = new QtkdWriteService(db);
        Assert.Equal(404, (await service.AddPriceItemAsync(99, new("SVC-001", 100m), default)).StatusCode);
        var list = await service.CreatePriceListAsync(new("Bảng giá mới", new DateOnly(2026, 11, 1)), default);
        Assert.Equal(201, list.StatusCode);
        Assert.Equal(400, (await service.AddPriceItemAsync(list.Value!.Id, new("SVC-001", 0m), default)).StatusCode);
        Assert.Equal(201, (await service.AddPriceItemAsync(list.Value.Id, new("SVC-001", 100m), default)).StatusCode);
        Assert.Single(await db.PriceItems.ToListAsync());
    }
}
