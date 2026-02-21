using FluentValidation;
using LibraryManagement.Application.Authors.Commands;

namespace LibraryManagement.Application.Authors.Validators;

public class DeleteAuthorCommandValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Author ID is required.");
    }
}
