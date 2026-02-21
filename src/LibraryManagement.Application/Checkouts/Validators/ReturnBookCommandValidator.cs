using FluentValidation;
using LibraryManagement.Application.Checkouts.Commands;

namespace LibraryManagement.Application.Checkouts.Validators;

public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator()
    {
        RuleFor(x => x.CheckoutId)
            .NotEmpty().WithMessage("Checkout ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}
