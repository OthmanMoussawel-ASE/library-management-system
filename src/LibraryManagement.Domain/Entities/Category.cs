using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual ICollection<BookCategory> BookCategories { get; set; } = [];
}
