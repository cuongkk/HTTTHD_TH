using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public sealed class ContractsController(IQtkdReadService read, IQtkdWriteService write) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var contract = await read.GetContractAsync(id, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }

    [HttpPost]
    public async Task<ActionResult<ContractResponse>> Create(
        CreateContractRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.CreateContractAsync(request, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ContractResponse>> Update(
        int id, UpdateContractRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.UpdateContractAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ContractResponse>> Submit(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await write.SubmitContractAsync(id, cancellationToken));
}
