using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Books.Queries;

public record GetBookByIdQuery(Guid Id) : IRequest<Result<BookDto>>;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (book is null || book.IsDeleted)
            return Result<BookDto>.NotFound("Book not found.");

        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }
}
