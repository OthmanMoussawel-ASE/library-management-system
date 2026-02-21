using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using MediatR;

namespace LibraryManagement.Application.Auth.Commands;

public record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RevokeTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        return await _identityService.RevokeTokenAsync(command.RefreshToken, cancellationToken);
    }
}
