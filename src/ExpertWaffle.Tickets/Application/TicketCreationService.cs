using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public sealed class TicketCreationService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IClock _clock;
    private readonly IEventBus _eventBus;

    public TicketCreationService(
        ITicketRepository ticketRepository,
        IClock clock,
        IEventBus eventBus)
    {
        _ticketRepository = ticketRepository;
        _clock = clock;
        _eventBus = eventBus;
    }

    public async Task<Guid> CreateAsync(
        string title,
        string description,
        TicketPriority priority,
        string? taskType,
        string? payloadJson,
        DateTimeOffset? scheduledAt,
        int maxRetryCount = 3,
        CancellationToken cancellationToken = default)
    {
        var ticket = new Ticket(
            Guid.NewGuid(),
            title,
            description,
            priority,
            _clock.UtcNow,
            taskType,
            payloadJson,
            scheduledAt,
            maxRetryCount);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _eventBus.PublishAsync(new TicketCreatedEvent(ticket.Id, _clock.UtcNow), cancellationToken);

        return ticket.Id;
    }
}
