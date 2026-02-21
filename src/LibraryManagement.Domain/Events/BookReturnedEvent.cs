using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Events;

public sealed record BookReturnedEvent(Guid BookId) : IDomainEvent;
