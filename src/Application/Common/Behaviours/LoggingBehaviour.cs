﻿using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Common.Behaviours;
public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IAuthService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IAuthService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("HotelManagement Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
