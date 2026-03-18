using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Tests.Domain;

public class TicketTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Ticket CreateOpenTicket(int maxRetryCount = 3)
    {
        return new Ticket(
            Guid.NewGuid(),
            "Test Ticket",
            "Description",
            TicketPriority.Normal,
            Now,
            "test_task",
            null,
            null,
            maxRetryCount);
    }

    [Fact]
    public void NewTicket_HasOpenStatus()
    {
        var ticket = CreateOpenTicket();
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void MarkReady_FromOpen_Succeeds()
    {
        var ticket = CreateOpenTicket();
        ticket.MarkReady(Now.AddSeconds(1));
        Assert.Equal(TicketStatus.Ready, ticket.Status);
    }

    [Fact]
    public void Start_FromReady_Succeeds()
    {
        var ticket = CreateOpenTicket();
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        Assert.Equal(TicketStatus.Running, ticket.Status);
    }

    [Fact]
    public void Complete_FromRunning_Succeeds()
    {
        var ticket = CreateOpenTicket();
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        ticket.Complete(Now.AddSeconds(3));
        Assert.Equal(TicketStatus.Completed, ticket.Status);
    }

    [Fact]
    public void Fail_FromRunning_IncrementsRetryCount()
    {
        var ticket = CreateOpenTicket();
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        ticket.Fail(Now.AddSeconds(3));
        Assert.Equal(TicketStatus.Failed, ticket.Status);
        Assert.Equal(1, ticket.RetryCount);
    }

    [Fact]
    public void Cancel_FromOpen_Succeeds()
    {
        var ticket = CreateOpenTicket();
        ticket.Cancel(Now.AddSeconds(1));
        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
    }

    [Fact]
    public void InvalidTransition_ThrowsInvalidOperationException()
    {
        var ticket = CreateOpenTicket();
        Assert.Throws<InvalidOperationException>(() => ticket.Start(Now.AddSeconds(1)));
    }

    [Fact]
    public void CanRetry_WhenRetryCountLessThanMax_ReturnsTrue()
    {
        var ticket = CreateOpenTicket(maxRetryCount: 3);
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        ticket.Fail(Now.AddSeconds(3));
        Assert.True(ticket.CanRetry());
    }

    [Fact]
    public void CanRetry_WhenRetryCountEqualsMax_ReturnsFalse()
    {
        var ticket = CreateOpenTicket(maxRetryCount: 1);
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        ticket.Fail(Now.AddSeconds(3));
        Assert.False(ticket.CanRetry());
    }

    [Fact]
    public void Retry_FromFailed_CanTransitionToReady()
    {
        var ticket = CreateOpenTicket();
        ticket.MarkReady(Now.AddSeconds(1));
        ticket.Start(Now.AddSeconds(2));
        ticket.Fail(Now.AddSeconds(3));
        ticket.MarkReady(Now.AddSeconds(4));
        Assert.Equal(TicketStatus.Ready, ticket.Status);
    }
}
