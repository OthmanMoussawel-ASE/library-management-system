using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Authors.Queries;

public record GetAllAuthorsQuery : IRequest<List<AuthorDto>>, ICachedQuery
{
    public string CacheKey => "authors_all";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}

public class GetAllAuthorsQueryHandler : IRequestHandler<GetAllAuthorsQuery, List<AuthorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllAuthorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<AuthorDto>> Handle(GetAllAuthorsQuery query, CancellationToken cancellationToken)
    {
        var authors = await _unitOfWork.Repository<Author>().GetAllAsync(cancellationToken);
        return _mapper.Map<List<AuthorDto>>(authors.Where(a => !a.IsDeleted).OrderBy(a => a.LastName));
    }
}
