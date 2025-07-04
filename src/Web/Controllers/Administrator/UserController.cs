﻿using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Security;
using HotelManagement.Application.Users.Commands;
using HotelManagement.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Administrator;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public class UserController(ISender mediator, IUserService userService) : ControllerBase
{
    private readonly ISender _mediator = mediator;
    private readonly IUserService _userService = userService;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var id = await _mediator.Send(new CreateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { id }, new { id });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
    {
        await _mediator.Send(new UpdateUserCommand(dto));
        return CreatedAtAction(nameof(GetUserById), new { dto.Id }, new { dto.Id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery { Id = id });
        return Ok(user);
    }
}
