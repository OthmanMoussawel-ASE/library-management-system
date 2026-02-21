using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.Events;

namespace LibraryManagement.Domain.Entities;

public class Book : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public int TotalCopies { get; set; } = 1;
    public int AvailableCopies { get; set; } = 1;
    public DateTime? PublishedDate { get; set; }
    public string? Publisher { get; set; }
    public int? PageCount { get; set; }
    public string? Language { get; set; } = "English";

    public Guid AuthorId { get; set; }
    public virtual Author Author { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ICollection<BookCategory> BookCategories { get; set; } = [];
    public virtual ICollection<CheckoutRecord> CheckoutRecords { get; set; } = [];

    public bool IsAvailable => AvailableCopies > 0;

    public void Checkout()
    {
        if (AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available for checkout.");

        AvailableCopies--;
        AddDomainEvent(new BookCheckedOutEvent(Id));
    }

    public void Return()
    {
        if (AvailableCopies >= TotalCopies)
            throw new InvalidOperationException("All copies are already returned.");

        AvailableCopies++;
        AddDomainEvent(new BookReturnedEvent(Id));
    }
}
