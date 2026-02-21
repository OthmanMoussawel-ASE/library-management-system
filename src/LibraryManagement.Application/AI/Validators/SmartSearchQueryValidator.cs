using FluentValidation;
using LibraryManagement.Application.AI.Queries;

namespace LibraryManagement.Application.AI.Validators;

public class SmartSearchQueryValidator : AbstractValidator<SmartSearchQuery>
{
    public SmartSearchQueryValidator()
    {
        RuleFor(x => x.NaturalLanguageQuery)
            .NotEmpty().WithMessage("Search query is required.")
            .MaximumLength(500).WithMessage("Search query must not exceed 500 characters.");
    }
}
