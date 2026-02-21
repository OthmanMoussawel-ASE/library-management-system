using FluentValidation;
using LibraryManagement.Application.Auth.Commands;

namespace LibraryManagement.Application.Auth.Validators;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
