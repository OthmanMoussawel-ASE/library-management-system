using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.ValueObjects;

namespace LibraryManagement.Domain.Entities;

public class CheckoutRecord : BaseEntity
{
    public Guid BookId { get; set; }
    public virtual Book Book { get; set; } = null!;

    public Guid PatronId { get; set; }
    public virtual Patron Patron { get; set; } = null!;

    public DateTime CheckedOutAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public CheckoutStatus Status { get; set; } = CheckoutStatus.Active;
    public string? Notes { get; set; }

    public bool IsOverdue => Status == CheckoutStatus.Active && DueDate < DateTime.UtcNow;
}
