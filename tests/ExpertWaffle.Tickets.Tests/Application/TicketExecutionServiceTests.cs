using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;
using ExpertWaffle.Tickets.Infrastructure;

namespace ExpertWaffle.Tickets.Tests.Application;

public class TicketExecutionServiceTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    private sealed class SuccessHandler : ITicketTaskHandler
    {
        public string TaskType => "success_task";
        public Task HandleAsync(TicketTaskContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FailingHandler : ITicketTaskHandler
    {
        public string TaskType => "fail_task";
        public Task HandleAsync(TicketTaskContext context, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Task failed");
    }

    private static (TicketExecutionService service, InMemoryTicketRepository ticketRepo, InMemoryTicketExecutionRepository execRepo, InMemoryEventBus eventBus) CreateService(ITicketTaskHandler handler)
    {
        var ticketRepo = new InMemoryTicketRepository();
        var execRepo = new InMemoryTicketExecutionRepository();
        var eventBus = new InMemoryEventBus();
        var clock = new FixedClock { UtcNow = Now };
        var resolver = new DefaultTicketTaskHandlerResolver(new[] { handler });
        var txManager = new NoOpTransactionManager();
        var serviceProvider = new EmptyServiceProvider();

        var service = new TicketExecutionService(
            ticketRepo,
            execRepo,
            resolver,
            txManager,
            clock,
            eventBus,
            serviceProvider);

        return (service, ticketRepo, execRepo, eventBus);
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static async Task<Ticket> CreateReadyTicket(ITicketRepository repo, string taskType)
    {
        var ticket = new Ticket(
            Guid.NewGuid(),
            "Test",
            "Description",
            TicketPriority.Normal,
            Now,
            taskType,
            null,
            null,
            3);
        ticket.MarkReady(Now);
        await repo.AddAsync(ticket, CancellationToken.None);
        return ticket;
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessHandler_CompletesTicket()
    {
        var (service, ticketRepo, execRepo, eventBus) = CreateService(new SuccessHandler());
        var ticket = await CreateReadyTicket(ticketRepo, "success_task");

        await service.ExecuteAsync(ticket.Id, CancellationToken.None);

        var updated = await ticketRepo.FindByIdAsync(ticket.Id, CancellationToken.None);
        Assert.Equal(TicketStatus.Completed, updated!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingHandler_FailsTicket()
    {
        var (service, ticketRepo, execRepo, eventBus) = CreateService(new FailingHandler());
        var ticket = await CreateReadyTicket(ticketRepo, "fail_task");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(ticket.Id, CancellationToken.None));

        var updated = await ticketRepo.FindByIdAsync(ticket.Id, CancellationToken.None);
        Assert.Equal(TicketStatus.Failed, updated!.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessHandler_PublishesStartedAndCompletedEvents()
    {
        var (service, ticketRepo, _, eventBus) = CreateService(new SuccessHandler());
        var ticket = await CreateReadyTicket(ticketRepo, "success_task");

        await service.ExecuteAsync(ticket.Id, CancellationToken.None);

        Assert.Contains(eventBus.PublishedEvents, e => e is TicketStartedEvent);
        Assert.Contains(eventBus.PublishedEvents, e => e is TicketCompletedEvent);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingHandler_PublishesFailedEvent()
    {
        var (service, ticketRepo, _, eventBus) = CreateService(new FailingHandler());
        var ticket = await CreateReadyTicket(ticketRepo, "fail_task");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(ticket.Id, CancellationToken.None));

        Assert.Contains(eventBus.PublishedEvents, e => e is TicketFailedEvent);
    }

    [Fact]
    public async Task ExecuteAsync_TicketNotFound_ThrowsInvalidOperationException()
    {
        var (service, _, _, _) = CreateService(new SuccessHandler());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_TicketNotReady_ThrowsInvalidOperationException()
    {
        var (service, ticketRepo, _, _) = CreateService(new SuccessHandler());
        var ticket = new Ticket(
            Guid.NewGuid(),
            "Test",
            "Description",
            TicketPriority.Normal,
            Now,
            "success_task",
            null,
            null,
            3);
        await ticketRepo.AddAsync(ticket, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(ticket.Id, CancellationToken.None));
    }
}
