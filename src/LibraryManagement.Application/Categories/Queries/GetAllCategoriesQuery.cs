using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>, ICachedQuery
{
    public string CacheKey => "categories_all";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync(cancellationToken);
        return _mapper.Map<List<CategoryDto>>(categories.OrderBy(c => c.Name));
    }
}
