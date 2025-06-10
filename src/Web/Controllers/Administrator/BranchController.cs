using Microsoft.AspNetCore.Http;
using HotelManagement.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Application.Common.Interfaces.Administrator;
using MediatR;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Users.Commands.CreateUser;
using HotelManagement.Application.Branches.Commands.CreateBranch;

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
        return CreatedAtAction(nameof(CreateBranch), new { id }, dto);
    }
}
