using LibraryManagement.Domain.ValueObjects;

namespace LibraryManagement.Application.Checkouts.DTOs;

public class CheckoutRecordDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public Guid PatronId { get; set; }
    public string PatronName { get; set; } = string.Empty;
    public string PatronEmail { get; set; } = string.Empty;
    public DateTime CheckedOutAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public CheckoutStatus Status { get; set; }
    public bool IsOverdue { get; set; }
    public string? Notes { get; set; }
}

public class CheckoutRequest
{
    public Guid BookId { get; set; }
    public int DueDays { get; set; } = 14;
    public string? Notes { get; set; }
}

public class ReturnRequest
{
    public Guid CheckoutId { get; set; }
    public string? Notes { get; set; }
}
