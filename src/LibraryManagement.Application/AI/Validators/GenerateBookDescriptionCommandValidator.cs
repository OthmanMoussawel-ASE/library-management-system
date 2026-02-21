using FluentValidation;
using LibraryManagement.Application.AI.Commands;

namespace LibraryManagement.Application.AI.Validators;

public class GenerateBookDescriptionCommandValidator : AbstractValidator<GenerateBookDescriptionCommand>
{
    public GenerateBookDescriptionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(200).WithMessage("Author must not exceed 200 characters.");
    }
}
