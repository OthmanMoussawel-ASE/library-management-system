using FluentAssertions;
using LibraryManagement.Application.Books.Queries;
using LibraryManagement.Tests.Common;

namespace LibraryManagement.Tests.Books;

public class GetBookByIdQueryTests : IDisposable
{
    private readonly TestFixture _fixture;

    public GetBookByIdQueryTests()
    {
        _fixture = new TestFixture();
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnBook()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync("George", "Orwell");
        var book = await _fixture.SeedBookAsync(author, "1984");

        var handler = new GetBookByIdQueryHandler(_fixture.UnitOfWork, _fixture.Mapper);

        // Act
        var result = await handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(book.Id);
        result.Value.Title.Should().Be("1984");
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new GetBookByIdQueryHandler(_fixture.UnitOfWork, _fixture.Mapper);

        // Act
        var result = await handler.Handle(new GetBookByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithDeletedBook_ShouldReturnNotFound()
    {
        // Arrange
        var author = await _fixture.SeedAuthorAsync();
        var book = await _fixture.SeedBookAsync(author, "Deleted Book");
        
        book.IsDeleted = true;
        _fixture.Context.Books.Update(book);
        await _fixture.Context.SaveChangesAsync();

        var handler = new GetBookByIdQueryHandler(_fixture.UnitOfWork, _fixture.Mapper);

        // Act
        var result = await handler.Handle(new GetBookByIdQuery(book.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
