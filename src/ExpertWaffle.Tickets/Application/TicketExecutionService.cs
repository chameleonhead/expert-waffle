using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public sealed class TicketExecutionService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketExecutionRepository _executionRepository;
    private readonly ITicketTaskHandlerResolver _handlerResolver;
    private readonly ITransactionManager _transactionManager;
    private readonly IClock _clock;
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;

    public TicketExecutionService(
        ITicketRepository ticketRepository,
        ITicketExecutionRepository executionRepository,
        ITicketTaskHandlerResolver handlerResolver,
        ITransactionManager transactionManager,
        IClock clock,
        IEventBus eventBus,
        IServiceProvider serviceProvider)
    {
        _ticketRepository = ticketRepository;
        _executionRepository = executionRepository;
        _handlerResolver = handlerResolver;
        _transactionManager = transactionManager;
        _clock = clock;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.FindByIdAsync(ticketId, cancellationToken)
            ?? throw new InvalidOperationException($"Ticket not found: {ticketId}");

        if (ticket.Status != TicketStatus.Ready)
            throw new InvalidOperationException($"Ticket is not ready: {ticket.Status}");

        var startedAt = _clock.UtcNow;
        var execution = new TicketExecution(Guid.NewGuid(), ticket.Id, startedAt);

        await _executionRepository.AddAsync(execution, cancellationToken);

        await using var tx = await _transactionManager.BeginAsync(cancellationToken);

        try
        {
            ticket.Start(_clock.UtcNow);
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            await _eventBus.PublishAsync(new TicketStartedEvent(ticket.Id, _clock.UtcNow), cancellationToken);

            var handler = _handlerResolver.Resolve(ticket.TaskType ?? throw new InvalidOperationException("TaskType is null"));
            var context = new TicketTaskContext(ticket, _serviceProvider);

            await handler.HandleAsync(context, cancellationToken);

            ticket.Complete(_clock.UtcNow);
            execution.Complete(_clock.UtcNow);

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            await _executionRepository.UpdateAsync(execution, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            await _eventBus.PublishAsync(new TicketCompletedEvent(ticket.Id, _clock.UtcNow), cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);

            ticket.Fail(_clock.UtcNow);
            execution.Fail(
                _clock.UtcNow,
                ex.GetType().Name,
                ex.Message,
                ex.ToString());

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            await _executionRepository.UpdateAsync(execution, cancellationToken);

            await _eventBus.PublishAsync(
                new TicketFailedEvent(ticket.Id, _clock.UtcNow, ex.GetType().Name, ex.Message),
                cancellationToken);

            throw;
        }
    }
}
