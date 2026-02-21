using FluentAssertions;
using LibraryManagement.Application.Books.Commands;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Tests.Common;
using Moq;

namespace LibraryManagement.Tests.Books;

public class UpdateBookCommandTests : IDisposable
{
    private readonly TestFixture _fixture;

    public UpdateBookCommandTests()
    {
        _fixture = new TestFixture();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateBook()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author, "Original Title");

        var request = new UpdateBookRequest
        {
            Title = "Updated Title",
            ISBN = book.ISBN,
            Description = "Updated description",
            TotalCopies = 10,
            AuthorId = author.Id,
            Language = "English"
        };

        var handler = new UpdateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new UpdateBookCommand(book.Id, request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Updated Title");
        result.Value.Description.Should().Be("Updated description");
        result.Value.TotalCopies.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithInvalidBookId_ShouldReturnNotFound()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();

        var request = new UpdateBookRequest
        {
            Title = "Some Title",
            TotalCopies = 5,
            AuthorId = author.Id
        };

        var handler = new UpdateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new UpdateBookCommand(Guid.NewGuid(), request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenIncreasingCopies_ShouldIncreaseAvailableCopies()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author);
        var originalAvailable = book.AvailableCopies;
        var originalTotal = book.TotalCopies;

        var request = new UpdateBookRequest
        {
            Title = book.Title,
            TotalCopies = originalTotal + 5,
            AuthorId = author.Id
        };

        var handler = new UpdateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new UpdateBookCommand(book.Id, request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCopies.Should().Be(originalTotal + 5);
        result.Value.AvailableCopies.Should().Be(originalAvailable + 5);
    }

    [Fact]
    public async Task Handle_WhenDecreasingCopies_ShouldNotGoBelowZero()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author);
        book.AvailableCopies = 2;
        _fixture.Context.Books.Update(book);
        await _fixture.Context.SaveChangesAsync();

        var request = new UpdateBookRequest
        {
            Title = book.Title,
            TotalCopies = 0,
            AuthorId = author.Id
        };

        var handler = new UpdateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new UpdateBookCommand(book.Id, request), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AvailableCopies.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author);

        var request = new UpdateBookRequest
        {
            Title = "Cache Test",
            TotalCopies = book.TotalCopies,
            AuthorId = author.Id
        };

        var handler = new UpdateBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.Mapper,
            _fixture.MockCacheService.Object);

        // Act
        await handler.Handle(new UpdateBookCommand(book.Id, request), CancellationToken.None);

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
