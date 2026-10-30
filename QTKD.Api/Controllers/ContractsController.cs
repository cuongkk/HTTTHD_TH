using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public sealed class ContractsController(IQtkdReadService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var contract = await service.GetContractAsync(id, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }
}
