using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;
using ExpertWaffle.Tickets.Infrastructure;

namespace ExpertWaffle.Tickets.Tests.Infrastructure;

public class InMemoryTicketRepositoryTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AddAsync_And_FindByIdAsync_ReturnsTicket()
    {
        var repo = new InMemoryTicketRepository();
        var ticket = new Ticket(Guid.NewGuid(), "T", "D", TicketPriority.Normal, Now, "t", null, null, 3);

        await repo.AddAsync(ticket, CancellationToken.None);
        var found = await repo.FindByIdAsync(ticket.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(ticket.Id, found.Id);
    }

    [Fact]
    public async Task FindByIdAsync_NotFound_ReturnsNull()
    {
        var repo = new InMemoryTicketRepository();
        var found = await repo.FindByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Null(found);
    }

    [Fact]
    public async Task FindReadyTicketsAsync_ReturnsOnlyReadyTickets()
    {
        var repo = new InMemoryTicketRepository();

        var openTicket = new Ticket(Guid.NewGuid(), "Open", "D", TicketPriority.Normal, Now, "t", null, null, 3);
        var readyTicket = new Ticket(Guid.NewGuid(), "Ready", "D", TicketPriority.Normal, Now, "t", null, null, 3);
        readyTicket.MarkReady(Now);

        await repo.AddAsync(openTicket, CancellationToken.None);
        await repo.AddAsync(readyTicket, CancellationToken.None);

        var result = await repo.FindReadyTicketsAsync(Now, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(readyTicket.Id, result[0].Id);
    }

    [Fact]
    public async Task FindOpenScheduledTicketsDueAsync_ReturnsDueTickets()
    {
        var repo = new InMemoryTicketRepository();

        var due = new Ticket(Guid.NewGuid(), "Due", "D", TicketPriority.Normal, Now, "t", null, Now.AddHours(-1), 3);
        var notDue = new Ticket(Guid.NewGuid(), "NotDue", "D", TicketPriority.Normal, Now, "t", null, Now.AddHours(1), 3);

        await repo.AddAsync(due, CancellationToken.None);
        await repo.AddAsync(notDue, CancellationToken.None);

        var result = await repo.FindOpenScheduledTicketsDueAsync(Now, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(due.Id, result[0].Id);
    }
}
