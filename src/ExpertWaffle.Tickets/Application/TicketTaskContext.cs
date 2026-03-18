using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Application;

public sealed class TicketTaskContext
{
    public Ticket Ticket { get; }
    public IServiceProvider Services { get; }
    public IDictionary<string, object> Items { get; }

    public TicketTaskContext(Ticket ticket, IServiceProvider services)
    {
        Ticket = ticket;
        Services = services;
        Items = new Dictionary<string, object>();
    }
}
