namespace LibraryManagement.Application.Common.Interfaces;

public interface IAIService
{
    Task<string> GenerateBookDescriptionAsync(string title, string author, CancellationToken cancellationToken = default);
    Task<List<string>> GetBookRecommendationsAsync(List<string> previousBooks, CancellationToken cancellationToken = default);
    Task<List<string>> CategorizeBookAsync(string title, string author, string? description, List<string>? existingCategories = null, CancellationToken cancellationToken = default);
    Task<string> SmartSearchAsync(string naturalLanguageQuery, CancellationToken cancellationToken = default);
    Task<List<string>> MatchBooksFromCatalogAsync(List<string> userBooks, List<string> catalogBooks, CancellationToken cancellationToken = default);
    bool IsAvailable { get; }
}
