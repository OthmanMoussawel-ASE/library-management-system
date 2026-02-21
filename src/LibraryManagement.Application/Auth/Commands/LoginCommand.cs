using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using MediatR;

namespace LibraryManagement.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(command.Email, command.Password, cancellationToken);
    }
}
