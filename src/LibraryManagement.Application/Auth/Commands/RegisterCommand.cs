using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using MediatR;

namespace LibraryManagement.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Result<AuthResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(command.Email, command.Password, command.FirstName, command.LastName, cancellationToken);
    }
}
