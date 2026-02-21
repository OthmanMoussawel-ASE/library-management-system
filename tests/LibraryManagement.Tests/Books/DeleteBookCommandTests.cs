using FluentAssertions;
using LibraryManagement.Application.Books.Commands;
using LibraryManagement.Tests.Common;
using Moq;

namespace LibraryManagement.Tests.Books;

public class DeleteBookCommandTests : IDisposable
{
    private readonly TestFixture _fixture;

    public DeleteBookCommandTests()
    {
        _fixture = new TestFixture();
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldDeleteBook()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author, "Book To Delete");

        var handler = new DeleteBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new DeleteBookCommand(book.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedBook = await _fixture.Context.Books.FindAsync(book.Id);
        deletedBook.Should().NotBeNull();
        deletedBook!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new DeleteBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.MockCacheService.Object);

        // Act
        var result = await handler.Handle(new DeleteBookCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author);

        var handler = new DeleteBookCommandHandler(
            _fixture.UnitOfWork,
            _fixture.MockCacheService.Object);

        // Act
        await handler.Handle(new DeleteBookCommand(book.Id), CancellationToken.None);

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
