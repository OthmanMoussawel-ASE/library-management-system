using FluentAssertions;
using LibraryManagement.Application.Books.Commands;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Tests.Common;
using Moq;

namespace LibraryManagement.Tests.Books;

public class CreateBookCommandTests : IDisposable
{
    private readonly TestFixture _fixture;

    public CreateBookCommandTests()
    {
        _fixture = new TestFixture();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateBook()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var category = await _fixture.SeedCategoryAsync();

        var request = new CreateBookRequest
        {
            Title = "New Book",
            ISBN = "978-1234567890",
            Description = "A great book",
            TotalCopies = 3,
            AuthorId = author.Id,
            CategoryIds = [category.Id],
            Language = "English"
        };

        var handler = new CreateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockAIService.Object,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new CreateBookCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("New Book");
        result.Value.ISBN.Should().Be("978-1234567890");
        result.Value.TotalCopies.Should().Be(3);
        result.Value.AvailableCopies.Should().Be(3);
        result.Value.AuthorId.Should().Be(author.Id);
    }

    [Fact]
    public async Task Handle_WithDuplicateISBN_ShouldReturnFailure()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var existingBook = await _fixture.SeedBookAsync(author);

        var request = new CreateBookRequest
        {
            Title = "Another Book",
            ISBN = existingBook.ISBN,
            TotalCopies = 1,
            AuthorId = author.Id
        };

        var handler = new CreateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockAIService.Object,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new CreateBookCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ISBN already exists");
    }

    [Fact]
    public async Task Handle_WithInvalidAuthor_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateBookRequest
        {
            Title = "Book Without Author",
            TotalCopies = 1,
            AuthorId = Guid.NewGuid()
        };

        var handler = new CreateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockAIService.Object,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new CreateBookCommand(request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Author not found");
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();

        var request = new CreateBookRequest
        {
            Title = "Cache Test Book",
            TotalCopies = 1,
            AuthorId = author.Id
        };

        var handler = new CreateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockAIService.Object,
            _fixture.MockCacheService.Object);

        // Act
        await handler.Handle(new CreateBookCommand(request), CancellationToken.None);

        // Assert
        _fixture.MockCacheService.Verify(
            x => x.RemoveByPrefixAsync("books_", It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
