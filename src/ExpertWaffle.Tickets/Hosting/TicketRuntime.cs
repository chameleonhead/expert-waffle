using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Scheduling;

namespace ExpertWaffle.Tickets.Hosting;

public sealed class TicketRuntime
{
    private readonly TicketSchedulerService _schedulerService;
    private readonly ITicketRepository _ticketRepository;
    private readonly TicketExecutionService _executionService;
    private readonly IClock _clock;
    private readonly TimeSpan _pollingInterval;

    public TicketRuntime(
        TicketSchedulerService schedulerService,
        ITicketRepository ticketRepository,
        TicketExecutionService executionService,
        IClock clock,
        TimeSpan? pollingInterval = null)
    {
        _schedulerService = schedulerService;
        _ticketRepository = ticketRepository;
        _executionService = executionService;
        _clock = clock;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _schedulerService.PromoteReadyTicketsAsync(cancellationToken);

            var readyTickets = await _ticketRepository.FindReadyTicketsAsync(_clock.UtcNow, cancellationToken);

            foreach (var ticket in readyTickets.OrderByDescending(x => x.Priority).ThenBy(x => x.CreatedAt))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await _executionService.ExecuteAsync(ticket.Id, cancellationToken);
                }
                catch
                {
                    // Execution errors are recorded in TicketExecution history.
                    // The loop continues to process remaining tickets.
                }
            }

            await Task.Delay(_pollingInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
