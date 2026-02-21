using System.Text.Json;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using MediatR;

namespace LibraryManagement.Application.AI.Queries;

public record SmartSearchQuery(string NaturalLanguageQuery) : IRequest<Result<SmartSearchResponse>>;

public class SmartSearchResponse
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public string? Keywords { get; set; }
}

public class SmartSearchQueryHandler : IRequestHandler<SmartSearchQuery, Result<SmartSearchResponse>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IAIService _aiService;

    public SmartSearchQueryHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<Result<SmartSearchResponse>> Handle(SmartSearchQuery query, CancellationToken cancellationToken)
    {
        if (!_aiService.IsAvailable)
            return Result<SmartSearchResponse>.Failure("AI service is not configured.");

        var rawResult = await _aiService.SmartSearchAsync(query.NaturalLanguageQuery, cancellationToken);

        try
        {
            var parsed = JsonSerializer.Deserialize<SmartSearchResponse>(rawResult ?? "{}", JsonOptions);
            return Result<SmartSearchResponse>.Success(parsed ?? new SmartSearchResponse());
        }
        catch
        {
            return Result<SmartSearchResponse>.Success(new SmartSearchResponse { Keywords = rawResult });
        }
    }
}
