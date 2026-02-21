using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Authors.Commands;

public record DeleteAuthorCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteAuthorCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeleteAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Repository<Author>().GetByIdAsync(command.Id, cancellationToken);
        if (author is null || author.IsDeleted)
            return Result<bool>.Failure("Author not found.", 404);

        _unitOfWork.Repository<Author>().Delete(author);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("authors_", cancellationToken);

        return Result<bool>.Success(true);
    }
}
