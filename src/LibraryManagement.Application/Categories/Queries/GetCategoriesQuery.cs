using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.Specifications;
using MediatR;

namespace LibraryManagement.Application.Categories.Queries;

public record GetCategoriesQuery(QueryParameters Parameters) : IRequest<PagedResult<CategoryDto>>, ICachedQuery
{
    public string CacheKey => $"categories_{Parameters.PageNumber}_{Parameters.PageSize}_{Parameters.SortBy}_{Parameters.SortDirection}_{Parameters.SearchTerm}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var spec = new CategoryFilterSpecification(query.Parameters);
        var countSpec = new CategoryFilterSpecification(query.Parameters, forCount: true);

        var categories = await _unitOfWork.Repository<Category>().FindAsync(spec, cancellationToken);
        var totalCount = await _unitOfWork.Repository<Category>().CountAsync(countSpec, cancellationToken);

        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

        return PagedResult<CategoryDto>.Create(categoryDtos, totalCount, query.Parameters.PageNumber, query.Parameters.PageSize);
    }
}

public class CategoryFilterSpecification : BaseSpecification<Category>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "createdat"
    };

    public CategoryFilterSpecification(QueryParameters parameters, bool forCount = false)
    {
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            AndCriteria(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTerm))
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
                case "name":
                    if (isDescending) ApplyOrderByDescending(c => c.Name);
                    else ApplyOrderBy(c => c.Name);
                    break;
                case "createdat":
                    if (isDescending) ApplyOrderByDescending(c => c.CreatedAt);
                    else ApplyOrderBy(c => c.CreatedAt);
                    break;
            }
        }
        else
        {
            ApplyOrderBy(c => c.Name);
        }
    }
}
