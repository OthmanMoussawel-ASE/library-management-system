using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.Specifications;
using MediatR;

namespace LibraryManagement.Application.Authors.Queries;

public record GetAuthorsQuery(QueryParameters Parameters) : IRequest<PagedResult<AuthorDto>>, ICachedQuery
{
    public string CacheKey => $"authors_{Parameters.PageNumber}_{Parameters.PageSize}_{Parameters.SortBy}_{Parameters.SortDirection}_{Parameters.SearchTerm}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
}

public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, PagedResult<AuthorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAuthorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuthorDto>> Handle(GetAuthorsQuery query, CancellationToken cancellationToken)
    {
        var spec = new AuthorFilterSpecification(query.Parameters);
        var countSpec = new AuthorFilterSpecification(query.Parameters, forCount: true);

        var authors = await _unitOfWork.Repository<Author>().FindAsync(spec, cancellationToken);
        var totalCount = await _unitOfWork.Repository<Author>().CountAsync(countSpec, cancellationToken);

        var authorDtos = _mapper.Map<List<AuthorDto>>(authors);

        return PagedResult<AuthorDto>.Create(authorDtos, totalCount, query.Parameters.PageNumber, query.Parameters.PageSize);
    }
}

public class AuthorFilterSpecification : BaseSpecification<Author>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "firstname", "lastname", "createdat"
    };

    public AuthorFilterSpecification(QueryParameters parameters, bool forCount = false)
    {
        Criteria = a => !a.IsDeleted;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            AndCriteria(a =>
                a.FirstName.ToLower().Contains(searchTerm) ||
                a.LastName.ToLower().Contains(searchTerm) ||
                (a.Biography != null && a.Biography.ToLower().Contains(searchTerm))
            );
        }

        if (forCount) return;

        ApplyPaging((parameters.PageNumber - 1) * parameters.PageSize, parameters.PageSize);

        var sortBy = parameters.SortBy;
        if (!string.IsNullOrEmpty(sortBy) && AllowedSortFields.Contains(sortBy))
        {
            var isDescending = parameters.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            switch (sortBy.ToLower())
            {
                case "firstname":
                    if (isDescending) ApplyOrderByDescending(a => a.FirstName);
                    else ApplyOrderBy(a => a.FirstName);
                    break;
                case "lastname":
                    if (isDescending) ApplyOrderByDescending(a => a.LastName);
                    else ApplyOrderBy(a => a.LastName);
                    break;
                case "createdat":
                    if (isDescending) ApplyOrderByDescending(a => a.CreatedAt);
                    else ApplyOrderBy(a => a.CreatedAt);
                    break;
            }
        }
        else
        {
            ApplyOrderBy(a => a.LastName);
        }
    }
}
