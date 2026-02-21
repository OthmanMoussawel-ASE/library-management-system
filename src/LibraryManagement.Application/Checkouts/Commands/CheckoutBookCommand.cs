using AutoMapper;
using LibraryManagement.Application.Checkouts.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.ValueObjects;
using MediatR;

namespace LibraryManagement.Application.Checkouts.Commands;

public record CheckoutBookCommand(CheckoutRequest Request) : IRequest<Result<CheckoutRecordDto>>;

public class CheckoutBookCommandHandler : IRequestHandler<CheckoutBookCommand, Result<CheckoutRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUser;

    public CheckoutBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _currentUser = currentUser;
    }

    public async Task<Result<CheckoutRecordDto>> Handle(CheckoutBookCommand command, CancellationToken cancellationToken)
    {
        var patron = await _unitOfWork.Patrons.GetByUserIdAsync(_currentUser.UserId!, cancellationToken);
        if (patron is null)
            return Result<CheckoutRecordDto>.Failure("Patron profile not found.");

        var book = await _unitOfWork.Books.GetByIdAsync(command.Request.BookId, cancellationToken);
        if (book is null)
            return Result<CheckoutRecordDto>.NotFound("Book not found.");

        if (!book.IsAvailable)
            return Result<CheckoutRecordDto>.Failure("No copies available for checkout.");

        var existingCheckout = await _unitOfWork.Checkouts.GetActiveByBookAndPatronAsync(
            command.Request.BookId, patron.Id, cancellationToken);
        if (existingCheckout is not null)
            return Result<CheckoutRecordDto>.Failure("You already have this book checked out.");

        book.Checkout();

        var checkout = new CheckoutRecord
        {
            BookId = command.Request.BookId,
            PatronId = patron.Id,
            CheckedOutAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(command.Request.DueDays),
            Status = CheckoutStatus.Active,
            Notes = command.Request.Notes
        };

        await _unitOfWork.Checkouts.AddAsync(checkout, cancellationToken);
        _unitOfWork.Books.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("books_", cancellationToken);

        return Result<CheckoutRecordDto>.Success(_mapper.Map<CheckoutRecordDto>(checkout));
    }
}
