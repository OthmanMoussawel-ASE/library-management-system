using AutoMapper;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Common.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibraryManagement.Tests.Common;

public class TestFixture : IDisposable
{
    public AppDbContext Context { get; }
    public IUnitOfWork UnitOfWork { get; }
    public IMapper Mapper { get; }
    public Mock<IAIService> MockAIService { get; }
    public Mock<ICacheService> MockCacheService { get; }

    public TestFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();

        var mockMediator = new Mock<IMediator>();
        UnitOfWork = new UnitOfWork(Context, mockMediator.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Book, BookDto>()
                .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.Author != null ? s.Author.FullName : ""))
                .ForMember(d => d.Categories, opt => opt.MapFrom(s => s.BookCategories.Select(bc => bc.Category!.Name).ToList()))
                .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.AvailableCopies > 0));
            cfg.CreateMap<Author, AuthorDto>();
            cfg.CreateMap<Category, CategoryDto>();
        });
        Mapper = mapperConfig.CreateMapper();

        MockAIService = new Mock<IAIService>();
        MockAIService.Setup(x => x.IsAvailable).Returns(false);

        MockCacheService = new Mock<ICacheService>();
        MockCacheService.Setup(x => x.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public async Task<Author> SeedAuthorAsync(string firstName = "Test", string lastName = "Author")
    {
        var author = new Author
        {
            FirstName = firstName,
            LastName = lastName,
            Biography = "Test biography"
        };
        Context.Authors.Add(author);
        await Context.SaveChangesAsync();
        return author;
    }

    public async Task<Category> SeedCategoryAsync(string name = "Test Category")
    {
        var category = new Category
        {
            Name = name,
            Description = "Test description"
        };
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();
        return category;
    }

    public async Task<Book> SeedBookAsync(Author author, string title = "Test Book")
    {
        var book = new Book
        {
            Title = title,
            ISBN = $"978-{Random.Shared.Next(1000000000, 2000000000)}",
            Description = "Test description",
            TotalCopies = 5,
            AvailableCopies = 5,
            AuthorId = author.Id,
            Language = "English"
        };
        Context.Books.Add(book);
        await Context.SaveChangesAsync();
        return book;
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
