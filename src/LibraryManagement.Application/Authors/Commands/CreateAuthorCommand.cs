using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Authors.Commands;

public record CreateAuthorCommand(string FirstName, string LastName, string? Biography) : IRequest<Result<AuthorDto>>;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Result<AuthorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public CreateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<AuthorDto>> Handle(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = new Author
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Biography = command.Biography
        };

        await _unitOfWork.Repository<Author>().AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("authors_", cancellationToken);

        return Result<AuthorDto>.Success(_mapper.Map<AuthorDto>(author));
    }
}
