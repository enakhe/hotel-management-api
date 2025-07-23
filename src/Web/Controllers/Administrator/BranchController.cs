using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Core.Branches.Commands;
using HotelManagement.Application.Core.Branches.Queries;
using HotelManagement.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Administrator;

[ApiController]
[Route("api/admin/branches")]
[Authorize(Roles = "Administrator")]
public class BranchController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
    {
        var id = await _mediator.Send(new CreateBranchCommand(dto));
        return CreatedAtAction(nameof(GetBranchById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] CreateBranchDto dto)
    {
        await _mediator.Send(new UpdateBranchCommand(dto) { Id = id });
        return CreatedAtAction(nameof(GetBranchById), new { id }, new { id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        await _mediator.Send(new DeleteBranchCommand { Id = id });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBranches()
    {
        var branches = await _mediator.Send(new GetAllBranchesQuery { });
        return Ok(branches);
    }

    [HttpGet("{id}/users")]
    public async Task<IActionResult> GetUsersByBranch(Guid id)
    {
        var users = await _mediator.Send(new GetUsersByBranchQuery { Id = id });
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BranchDto>> GetBranchById(Guid id)
    {
        var branches = await _mediator.Send(new GetBranchByIdQuery { Id = id });
        return Ok(branches);
    }
}
