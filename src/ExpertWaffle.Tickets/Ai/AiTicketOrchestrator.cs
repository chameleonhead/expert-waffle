using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Ai;

public sealed class AiTicketOrchestrator
{
    private readonly IAiDecisionService _aiDecisionService;
    private readonly ITicketRepository _ticketRepository;
    private readonly IClock _clock;
    private readonly IEventBus _eventBus;

    public AiTicketOrchestrator(
        IAiDecisionService aiDecisionService,
        ITicketRepository ticketRepository,
        IClock clock,
        IEventBus eventBus)
    {
        _aiDecisionService = aiDecisionService;
        _ticketRepository = ticketRepository;
        _clock = clock;
        _eventBus = eventBus;
    }

    public async Task<Guid> CreateTicketFromAiAsync(string input, CancellationToken cancellationToken)
    {
        var proposal = await _aiDecisionService.ProposeTicketAsync(input, cancellationToken);

        var ticket = new Ticket(
            Guid.NewGuid(),
            proposal.Title,
            proposal.Description,
            proposal.Priority,
            _clock.UtcNow,
            proposal.TaskType,
            proposal.PayloadJson,
            proposal.ScheduledAt,
            maxRetryCount: 3);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _eventBus.PublishAsync(new TicketCreatedEvent(ticket.Id, _clock.UtcNow), cancellationToken);

        return ticket.Id;
    }

    public async Task<AiExecutionDecision> EvaluateAfterFailureAsync(
        Guid ticketId,
        string executionSummary,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.FindByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket not found: {ticketId}");

        return await _aiDecisionService.DecideNextActionAsync(ticket, executionSummary, cancellationToken);
    }
}
