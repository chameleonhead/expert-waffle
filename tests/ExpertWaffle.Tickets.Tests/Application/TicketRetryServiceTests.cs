using ExpertWaffle.Tickets.Application;
using ExpertWaffle.Tickets.Domain;
using ExpertWaffle.Tickets.Infrastructure;

namespace ExpertWaffle.Tickets.Tests.Application;

public class TicketRetryServiceTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    [Fact]
    public async Task RetryAsync_FromFailed_WithRemainingRetries_ReturnsTrue()
    {
        var repo = new InMemoryTicketRepository();
        var clock = new FixedClock { UtcNow = Now };
        var service = new TicketRetryService(repo, clock);

        var ticket = new Ticket(Guid.NewGuid(), "T", "D", TicketPriority.Normal, Now, "t", null, null, 3);
        ticket.MarkReady(Now);
        ticket.Start(Now);
        ticket.Fail(Now);
        await repo.AddAsync(ticket, CancellationToken.None);

        var result = await service.RetryAsync(ticket.Id, CancellationToken.None);

        Assert.True(result);
        var updated = await repo.FindByIdAsync(ticket.Id, CancellationToken.None);
        Assert.Equal(TicketStatus.Ready, updated!.Status);
    }

    [Fact]
    public async Task RetryAsync_FromFailed_WhenMaxRetriesExceeded_ReturnsFalse()
    {
        var repo = new InMemoryTicketRepository();
        var clock = new FixedClock { UtcNow = Now };
        var service = new TicketRetryService(repo, clock);

        var ticket = new Ticket(Guid.NewGuid(), "T", "D", TicketPriority.Normal, Now, "t", null, null, 1);
        ticket.MarkReady(Now);
        ticket.Start(Now);
        ticket.Fail(Now);
        await repo.AddAsync(ticket, CancellationToken.None);

        var result = await service.RetryAsync(ticket.Id, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task RetryAsync_TicketNotFailed_ThrowsInvalidOperationException()
    {
        var repo = new InMemoryTicketRepository();
        var clock = new FixedClock { UtcNow = Now };
        var service = new TicketRetryService(repo, clock);

        var ticket = new Ticket(Guid.NewGuid(), "T", "D", TicketPriority.Normal, Now, "t", null, null, 3);
        await repo.AddAsync(ticket, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RetryAsync(ticket.Id, CancellationToken.None));
    }
}
