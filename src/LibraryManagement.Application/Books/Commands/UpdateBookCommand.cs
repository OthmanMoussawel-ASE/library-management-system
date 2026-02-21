using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Application.Common.Models;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using MediatR;

namespace LibraryManagement.Application.Books.Commands;

public record UpdateBookCommand(Guid Id, UpdateBookRequest Request) : IRequest<Result<BookDto>>;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, Result<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public UpdateBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<Result<BookDto>> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdWithDetailsAsync(command.Id, cancellationToken);
        if (book is null)
            return Result<BookDto>.NotFound("Book not found.");

        var request = command.Request;

        var copiesDiff = request.TotalCopies - book.TotalCopies;

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Description = request.Description;
        book.CoverImageUrl = request.CoverImageUrl;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = Math.Max(0, book.AvailableCopies + copiesDiff);
        book.PublishedDate = request.PublishedDate;
        book.Publisher = request.Publisher;
        book.PageCount = request.PageCount;
        book.Language = request.Language;
        book.AuthorId = request.AuthorId;

        if (request.CategoryIds is not null)
        {
            book.BookCategories.Clear();
            foreach (var categoryId in request.CategoryIds)
            {
                book.BookCategories.Add(new BookCategory { BookId = book.Id, CategoryId = categoryId });
            }
        }

        _unitOfWork.Books.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("books_", cancellationToken);

        var updated = await _unitOfWork.Books.GetByIdWithDetailsAsync(book.Id, cancellationToken);
        return Result<BookDto>.Success(_mapper.Map<BookDto>(updated!));
    }
}
