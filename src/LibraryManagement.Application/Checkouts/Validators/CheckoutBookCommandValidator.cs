using FluentValidation;
using LibraryManagement.Application.Checkouts.Commands;

namespace LibraryManagement.Application.Checkouts.Validators;

public class CheckoutBookCommandValidator : AbstractValidator<CheckoutBookCommand>
{
    public CheckoutBookCommandValidator()
    {
        RuleFor(x => x.Request.BookId)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Request.DueDays)
            .InclusiveBetween(1, 90).WithMessage("Due days must be between 1 and 90.");

        RuleFor(x => x.Request.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}
