using FluentValidation;
using LibraryManagement.Application.Categories.Commands;

namespace LibraryManagement.Application.Categories.Validators;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
