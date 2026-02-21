using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.AI.Commands;

public record CategorizeBookCommand(string Title, string Author, string? Description) : IRequest<Result<CategorizeBookResponse>>;

public record CategorizeBookResponse(List<string> Existing, List<string> Suggested);

public class CategorizeBookCommandHandler : IRequestHandler<CategorizeBookCommand, Result<CategorizeBookResponse>>
{
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public CategorizeBookCommandHandler(IAIService aiService, IUnitOfWork unitOfWork)
    {
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategorizeBookResponse>> Handle(CategorizeBookCommand command, CancellationToken cancellationToken)
    {
        if (!_aiService.IsAvailable)
            return Result<CategorizeBookResponse>.Failure("AI service is not configured.");

        var allCategories = await _unitOfWork.Repository<Category>().GetAllAsync(cancellationToken);
        var existingCategoryNames = allCategories.Select(c => c.Name).ToList();

        var aiSuggestions = await _aiService.CategorizeBookAsync(
            command.Title,
            command.Author,
            command.Description,
            existingCategoryNames,
            cancellationToken);

        var existingMatches = new List<string>();
        var newSuggestions = new List<string>();

        foreach (var suggestion in aiSuggestions)
        {
            var match = existingCategoryNames.FirstOrDefault(c =>
                c.Equals(suggestion, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                existingMatches.Add(match);
            else
                newSuggestions.Add(suggestion);
        }

        return Result<CategorizeBookResponse>.Success(new CategorizeBookResponse(existingMatches, newSuggestions));
    }
}
