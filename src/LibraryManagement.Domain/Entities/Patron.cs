using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

public class Patron : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public virtual ICollection<CheckoutRecord> CheckoutRecords { get; set; } = [];
}
