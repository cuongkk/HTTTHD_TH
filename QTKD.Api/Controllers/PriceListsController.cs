using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/pricelists")]
public sealed class PriceListsController(IQtkdReadService read, IQtkdWriteService write) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PriceListResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await read.GetPriceListsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriceListResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var priceList = await read.GetPriceListAsync(id, cancellationToken);
        return priceList is null ? NotFound() : Ok(priceList);
    }

    [HttpGet("{id:int}/items")]
    public async Task<ActionResult<IReadOnlyList<PriceItemResponse>>> GetItems(int id, CancellationToken cancellationToken)
    {
        var priceList = await read.GetPriceListAsync(id, cancellationToken);
        return priceList is null ? NotFound() : Ok(priceList.Items);
    }

    [HttpPost]
    public async Task<ActionResult<PriceListResponse>> Create(
        CreatePriceListRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.CreatePriceListAsync(request, cancellationToken));

    [HttpPost("{id:int}/items")]
    public async Task<ActionResult<PriceItemResponse>> AddItem(
        int id, AddPriceItemRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.AddPriceItemAsync(id, request, cancellationToken));
}
