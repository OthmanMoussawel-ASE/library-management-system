using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

public class Author : BaseEntity, ISoftDeletable
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Biography { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ICollection<Book> Books { get; set; } = [];
}
