namespace ExpertWaffle.Tickets.Domain;

public sealed record TicketCreatedEvent(Guid TicketId, DateTimeOffset OccurredAt);
public sealed record TicketStartedEvent(Guid TicketId, DateTimeOffset OccurredAt);
public sealed record TicketCompletedEvent(Guid TicketId, DateTimeOffset OccurredAt);
public sealed record TicketFailedEvent(Guid TicketId, DateTimeOffset OccurredAt, string ErrorCode, string ErrorMessage);
public sealed record TicketCancelledEvent(Guid TicketId, DateTimeOffset OccurredAt);
