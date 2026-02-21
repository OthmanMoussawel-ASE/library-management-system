using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.Specifications;
using MediatR;

namespace LibraryManagement.Application.Books.Queries;

public record GetBooksQuery(QueryParameters Parameters) : IRequest<PagedResult<BookDto>>, ICachedQuery
{
    public string CacheKey => $"books_{Parameters.PageNumber}_{Parameters.PageSize}_{Parameters.SortBy}_{Parameters.SortDirection}_{Parameters.SearchTerm}_{(Parameters.Filters != null ? string.Join("_", Parameters.Filters.Select(f => $"{f.Key}:{f.Value}")) : "")}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}

public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PagedResult<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBooksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookDto>> Handle(GetBooksQuery query, CancellationToken cancellationToken)
    {
        var spec = new BookFilterSpecification(query.Parameters);
        var countSpec = new BookFilterSpecification(query.Parameters, forCount: true);

        var books = await _unitOfWork.Books.FindAsync(spec, cancellationToken);
        var totalCount = await _unitOfWork.Books.CountAsync(countSpec, cancellationToken);

        var bookDtos = _mapper.Map<List<BookDto>>(books);

        return PagedResult<BookDto>.Create(bookDtos, totalCount, query.Parameters.PageNumber, query.Parameters.PageSize);
    }
}

public class BookFilterSpecification : BaseSpecification<Book>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "title", "createdat", "publisheddate", "availablecopies"
    };

    public BookFilterSpecification(QueryParameters parameters, bool forCount = false)
    {
        AddInclude(b => b.Author);
        AddInclude(b => b.BookCategories);
        AddInclude("BookCategories.Category");

        Criteria = b => !b.IsDeleted;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchTerms = parameters.SearchTerm.ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 1)
                .ToArray();

            if (searchTerms.Length > 0)
            {
                AndCriteria(b =>
                    searchTerms.Any(term =>
                        b.Title.ToLower().Contains(term) ||
                        (b.ISBN != null && b.ISBN.ToLower().Contains(term)) ||
                        b.Author.FirstName.ToLower().Contains(term) ||
                        b.Author.LastName.ToLower().Contains(term) ||
                        (b.Description != null && b.Description.ToLower().Contains(term)) ||
                        b.BookCategories.Any(bc => bc.Category.Name.ToLower().Contains(term))
                    )
                );
            }
        }

        if (parameters.Filters is not null)
        {
            foreach (var filter in parameters.Filters)
            {
                switch (filter.Key.ToLower())
                {
                    case "authorid" when Guid.TryParse(filter.Value, out var authorId):
                        AndCriteria(b => b.AuthorId == authorId);
                        break;
                    case "language":
                        var lang = filter.Value.ToLower();
                        AndCriteria(b => b.Language != null && b.Language.ToLower() == lang);
                        break;
                    case "available" when bool.TryParse(filter.Value, out var available):
                        AndCriteria(b => available ? b.AvailableCopies > 0 : b.AvailableCopies == 0);
                        break;
                }
            }
        }

        if (forCount) return;

        ApplyPaging((parameters.PageNumber - 1) * parameters.PageSize, parameters.PageSize);

        var sortBy = parameters.SortBy;
        if (!string.IsNullOrEmpty(sortBy) && AllowedSortFields.Contains(sortBy))
        {
            var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            switch (sortBy.ToLower())
            {
                case "title":
                    if (isDescending) ApplyOrderByDescending(b => b.Title);
                    else ApplyOrderBy(b => b.Title);
                    break;
                case "createdat":
                    if (isDescending) ApplyOrderByDescending(b => b.CreatedAt);
                    else ApplyOrderBy(b => b.CreatedAt);
                    break;
                case "publisheddate":
                    if (isDescending) ApplyOrderByDescending(b => b.PublishedDate!);
                    else ApplyOrderBy(b => b.PublishedDate!);
                    break;
                case "availablecopies":
                    if (isDescending) ApplyOrderByDescending(b => b.AvailableCopies);
                    else ApplyOrderBy(b => b.AvailableCopies);
                    break;
            }
        }
        else
        {
            ApplyOrderByDescending(b => b.CreatedAt);
        }
    }
}
