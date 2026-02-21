using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Authors.Commands;

public record UpdateAuthorCommand(Guid Id, string FirstName, string LastName, string? Biography) : IRequest<Result<AuthorDto>>;

public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Result<AuthorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public UpdateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<AuthorDto>> Handle(UpdateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Repository<Author>().GetByIdAsync(command.Id, cancellationToken);
        if (author is null || author.IsDeleted)
            return Result<AuthorDto>.Failure("Author not found.", 404);

        author.FirstName = command.FirstName;
        author.LastName = command.LastName;
        author.Biography = command.Biography;

        _unitOfWork.Repository<Author>().Update(author);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("authors_", cancellationToken);

        return Result<AuthorDto>.Success(_mapper.Map<AuthorDto>(author));
    }
}
