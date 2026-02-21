using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.AI.Queries;

public record GetBookRecommendationsQuery : IRequest<Result<BookRecommendationsResponse>>;

public record BookRecommendationsResponse(
    List<RecommendedBook> FromLibrary,
    List<string> DiscoverMore
);

public record RecommendedBook(Guid Id, string Title, string Author, bool IsAvailable);

public class GetBookRecommendationsQueryHandler : IRequestHandler<GetBookRecommendationsQuery, Result<BookRecommendationsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUser;

    public GetBookRecommendationsQueryHandler(IUnitOfWork unitOfWork, IAIService aiService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _currentUser = currentUser;
    }

    public async Task<Result<BookRecommendationsResponse>> Handle(GetBookRecommendationsQuery query, CancellationToken cancellationToken)
    {
        var patron = await _unitOfWork.Patrons.GetByUserIdAsync(_currentUser.UserId!, cancellationToken);
        if (patron is null)
            return Result<BookRecommendationsResponse>.Failure("Patron profile not found.");

        var fromLibrary = new List<RecommendedBook>();
        var discoverMore = new List<string>();

        var availableBooks = await _unitOfWork.Books.GetAllWithAuthorsAsync(cancellationToken);

        var checkouts = await _unitOfWork.Checkouts.GetAllByPatronIdAsync(patron.Id, cancellationToken);
        var checkedOutBookIds = checkouts
            .Where(c => c.Book is not null)
            .Select(c => c.BookId)
            .Distinct()
            .ToHashSet();

        var userBookTitles = checkouts
            .Where(c => c.Book is not null)
            .Select(c => c.Book.Title)
            .Distinct()
            .ToList();

        var notCheckedOut = availableBooks
            .Where(b => !checkedOutBookIds.Contains(b.Id))
            .ToList();

        if (notCheckedOut.Count > 0)
        {
            if (_aiService.IsAvailable && userBookTitles.Count > 0)
            {
                var catalogTitles = notCheckedOut.Select(b => b.Title).ToList();
                var aiPicks = await _aiService.MatchBooksFromCatalogAsync(userBookTitles, catalogTitles, cancellationToken);

                foreach (var title in aiPicks.Take(5))
                {
                    var book = notCheckedOut.FirstOrDefault(b =>
                        b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                    if (book != null)
                    {
                        fromLibrary.Add(new RecommendedBook(
                            book.Id,
                            book.Title,
                            (book.Author?.FirstName + " " + book.Author?.LastName).Trim(),
                            book.AvailableCopies > 0));
                    }
                }
            }

            if (fromLibrary.Count < 5)
            {
                var existingIds = fromLibrary.Select(r => r.Id).ToHashSet();
                var fillers = notCheckedOut
                    .Where(b => !existingIds.Contains(b.Id))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(5 - fromLibrary.Count)
                    .Select(b => new RecommendedBook(
                        b.Id,
                        b.Title,
                        (b.Author?.FirstName + " " + b.Author?.LastName).Trim(),
                        b.AvailableCopies > 0));
                fromLibrary.AddRange(fillers);
            }
        }

        if (_aiService.IsAvailable && userBookTitles.Count > 0)
        {
            var aiSuggestions = await _aiService.GetBookRecommendationsAsync(userBookTitles, cancellationToken);

            var catalogTitlesLower = availableBooks.Select(b => b.Title.ToLower()).ToHashSet();
            discoverMore = aiSuggestions
                .Where(s => !catalogTitlesLower.Contains(s.ToLower()))
                .Take(5)
                .ToList();
        }

        return Result<BookRecommendationsResponse>.Success(new BookRecommendationsResponse(fromLibrary, discoverMore));
    }
}
