using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using MediatR;

namespace LibraryManagement.Application.AI.Commands;

public record GenerateBookDescriptionCommand(string Title, string Author) : IRequest<Result<string>>;

public class GenerateBookDescriptionCommandHandler : IRequestHandler<GenerateBookDescriptionCommand, Result<string>>
{
    private readonly IAIService _aiService;

    public GenerateBookDescriptionCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<Result<string>> Handle(GenerateBookDescriptionCommand command, CancellationToken cancellationToken)
    {
        if (!_aiService.IsAvailable)
            return Result<string>.Failure("AI service is not configured.");

        var description = await _aiService.GenerateBookDescriptionAsync(command.Title, command.Author, cancellationToken);
        return Result<string>.Success(description);
    }
}
