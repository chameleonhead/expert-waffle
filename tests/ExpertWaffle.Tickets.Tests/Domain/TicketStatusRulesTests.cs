using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Tests.Domain;

public class TicketStatusRulesTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.Ready, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Cancelled, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Running, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Completed, false)]
    [InlineData(TicketStatus.Ready, TicketStatus.Running, true)]
    [InlineData(TicketStatus.Ready, TicketStatus.Cancelled, true)]
    [InlineData(TicketStatus.Ready, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Running, TicketStatus.Completed, true)]
    [InlineData(TicketStatus.Running, TicketStatus.Failed, true)]
    [InlineData(TicketStatus.Running, TicketStatus.Cancelled, true)]
    [InlineData(TicketStatus.Running, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Failed, TicketStatus.Ready, true)]
    [InlineData(TicketStatus.Failed, TicketStatus.Cancelled, true)]
    [InlineData(TicketStatus.Failed, TicketStatus.Running, false)]
    [InlineData(TicketStatus.Completed, TicketStatus.Ready, false)]
    [InlineData(TicketStatus.Cancelled, TicketStatus.Ready, false)]
    public void CanTransition_ReturnsExpected(TicketStatus current, TicketStatus next, bool expected)
    {
        Assert.Equal(expected, TicketStatusRules.CanTransition(current, next));
    }
}
