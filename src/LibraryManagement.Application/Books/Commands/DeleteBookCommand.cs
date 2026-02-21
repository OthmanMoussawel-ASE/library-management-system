using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Books.Commands;

public record DeleteBookCommand(Guid Id) : IRequest<Result>;

public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DeleteBookCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(command.Id, cancellationToken);
        if (book is null)
            return Result.NotFound("Book not found.");

        _unitOfWork.Books.Delete(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("books_", cancellationToken);

        return Result.Success();
    }
}
