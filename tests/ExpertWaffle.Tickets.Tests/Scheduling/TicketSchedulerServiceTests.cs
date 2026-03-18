using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;
using ExpertWaffle.Tickets.Infrastructure;
using ExpertWaffle.Tickets.Scheduling;

namespace ExpertWaffle.Tickets.Tests.Scheduling;

public class TicketSchedulerServiceTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    [Fact]
    public async Task PromoteReadyTicketsAsync_PromotesDueTickets()
    {
        var repo = new InMemoryTicketRepository();
        var clock = new FixedClock { UtcNow = Now };
        var scheduler = new TicketSchedulerService(repo, clock);

        // Due ticket
        var due = new Ticket(Guid.NewGuid(), "Due", "D", TicketPriority.Normal, Now, "t", null, Now.AddHours(-1), 3);
        // Not due yet
        var notDue = new Ticket(Guid.NewGuid(), "NotDue", "D", TicketPriority.Normal, Now, "t", null, Now.AddHours(1), 3);

        await repo.AddAsync(due, CancellationToken.None);
        await repo.AddAsync(notDue, CancellationToken.None);

        await scheduler.PromoteReadyTicketsAsync(CancellationToken.None);

        var updatedDue = await repo.FindByIdAsync(due.Id, CancellationToken.None);
        var updatedNotDue = await repo.FindByIdAsync(notDue.Id, CancellationToken.None);

        Assert.Equal(TicketStatus.Ready, updatedDue!.Status);
        Assert.Equal(TicketStatus.Open, updatedNotDue!.Status);
    }

    [Fact]
    public async Task PromoteReadyTicketsAsync_DoesNotPromoteTicketsWithoutScheduledAt()
    {
        var repo = new InMemoryTicketRepository();
        var clock = new FixedClock { UtcNow = Now };
        var scheduler = new TicketSchedulerService(repo, clock);

        var ticket = new Ticket(Guid.NewGuid(), "NoSchedule", "D", TicketPriority.Normal, Now, "t", null, null, 3);
        await repo.AddAsync(ticket, CancellationToken.None);

        await scheduler.PromoteReadyTicketsAsync(CancellationToken.None);

        var updated = await repo.FindByIdAsync(ticket.Id, CancellationToken.None);
        Assert.Equal(TicketStatus.Open, updated!.Status);
    }
}
