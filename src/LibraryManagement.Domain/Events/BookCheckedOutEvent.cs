using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Events;

public sealed record BookCheckedOutEvent(Guid BookId) : IDomainEvent;
