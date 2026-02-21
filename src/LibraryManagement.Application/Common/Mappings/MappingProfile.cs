using AutoMapper;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Application.Books.DTOs;
using LibraryManagement.Application.Checkouts.DTOs;

namespace LibraryManagement.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.Author != null ? s.Author.FullName : string.Empty))
            .ForMember(d => d.Categories, opt => opt.MapFrom(s => s.BookCategories.Select(bc => bc.Category.Name).ToList()))
            .ForMember(d => d.IsAvailable, opt => opt.MapFrom(s => s.IsAvailable));

        CreateMap<Author, AuthorDto>();
        CreateMap<Category, CategoryDto>();

        CreateMap<CheckoutRecord, CheckoutRecordDto>()
            .ForMember(d => d.BookTitle, opt => opt.MapFrom(s => s.Book != null ? s.Book.Title : string.Empty))
            .ForMember(d => d.PatronName, opt => opt.MapFrom(s => s.Patron != null ? s.Patron.FullName : string.Empty))
            .ForMember(d => d.PatronEmail, opt => opt.MapFrom(s => s.Patron != null ? s.Patron.Email : string.Empty))
            .ForMember(d => d.IsOverdue, opt => opt.MapFrom(s => s.IsOverdue));
    }
}
