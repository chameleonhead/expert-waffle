namespace ExpertWaffle.Tickets.Application;

public interface ITicketTaskHandlerResolver
{
    ITicketTaskHandler Resolve(string taskType);
}
