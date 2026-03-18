namespace ExpertWaffle.Tickets.Domain;

public static class TicketStatusRules
{
    public static bool CanTransition(TicketStatus current, TicketStatus next)
    {
        return current switch
        {
            TicketStatus.Open => next is TicketStatus.Ready or TicketStatus.Cancelled,
            TicketStatus.Ready => next is TicketStatus.Running or TicketStatus.Cancelled,
            TicketStatus.Running => next is TicketStatus.Completed or TicketStatus.Failed or TicketStatus.Cancelled,
            TicketStatus.Failed => next is TicketStatus.Ready or TicketStatus.Cancelled,
            TicketStatus.Completed => false,
            TicketStatus.Cancelled => false,
            _ => false
        };
    }
}
