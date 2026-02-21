using FluentValidation;
using LibraryManagement.Application.Books.Commands;

namespace LibraryManagement.Application.Books.Validators;

public class UpdateBookValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Request.ISBN)
            .MaximumLength(20).WithMessage("ISBN must not exceed 20 characters.")
            .Matches(@"^[\d\-X]+$").When(x => !string.IsNullOrEmpty(x.Request.ISBN))
            .WithMessage("ISBN must contain only digits, hyphens, and X.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        RuleFor(x => x.Request.TotalCopies)
            .GreaterThan(0).WithMessage("Total copies must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Total copies must not exceed 10000.");

        RuleFor(x => x.Request.AuthorId)
            .NotEmpty().WithMessage("Author is required.");
    }
}
