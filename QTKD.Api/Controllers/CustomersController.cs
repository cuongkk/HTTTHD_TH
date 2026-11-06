using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(IQtkdReadService read, IQtkdWriteService write) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await read.GetCustomersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await read.GetCustomerAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        CreateCustomerRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.CreateCustomerAsync(request, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Update(
        int id, UpdateCustomerRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.UpdateCustomerAsync(id, request, cancellationToken));
}
