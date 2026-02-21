using AutoMapper;
using LibraryManagement.Application.Checkouts.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.ValueObjects;
using MediatR;

namespace LibraryManagement.Application.Checkouts.Commands;

public record ReturnBookCommand(Guid CheckoutId, string? Notes) : IRequest<Result<CheckoutRecordDto>>;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, Result<CheckoutRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public ReturnBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    public async Task<Result<CheckoutRecordDto>> Handle(ReturnBookCommand command, CancellationToken cancellationToken)
    {
        var checkout = await _unitOfWork.Checkouts.GetByIdAsync(command.CheckoutId, cancellationToken);
        if (checkout is null)
            return Result<CheckoutRecordDto>.NotFound("Checkout record not found.");

        var isStaff = _currentUser.Role is "Admin" or "Librarian";

        if (!isStaff)
        {
            var patron = await _unitOfWork.Patrons.GetByUserIdAsync(_currentUser.UserId!, cancellationToken);
            if (patron is null)
                return Result<CheckoutRecordDto>.Failure("Patron profile not found.");

            if (checkout.PatronId != patron.Id)
                return Result<CheckoutRecordDto>.Failure("You can only return your own checkouts.", 403);
        }

        if (checkout.Status != CheckoutStatus.Active && checkout.Status != CheckoutStatus.Overdue)
            return Result<CheckoutRecordDto>.Failure("This book has already been returned.");

        var book = await _unitOfWork.Books.GetByIdAsync(checkout.BookId, cancellationToken);
        if (book is null)
            return Result<CheckoutRecordDto>.NotFound("Book not found.");

        book.Return();
        checkout.ReturnedAt = DateTime.UtcNow;
        checkout.Status = CheckoutStatus.Returned;
        checkout.Notes = command.Notes ?? checkout.Notes;

        _unitOfWork.Checkouts.Update(checkout);
        _unitOfWork.Books.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("books_", cancellationToken);

        return Result<CheckoutRecordDto>.Success(_mapper.Map<CheckoutRecordDto>(checkout));
    }
}
