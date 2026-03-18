using ExpertWaffle.Tickets.Application;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class DefaultTicketTaskHandlerResolver : ITicketTaskHandlerResolver
{
    private readonly IReadOnlyDictionary<string, ITicketTaskHandler> _handlers;

    public DefaultTicketTaskHandlerResolver(IEnumerable<ITicketTaskHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.TaskType, StringComparer.OrdinalIgnoreCase);
    }

    public ITicketTaskHandler Resolve(string taskType)
    {
        if (_handlers.TryGetValue(taskType, out var handler))
            return handler;

        throw new InvalidOperationException($"No handler registered for task type: {taskType}");
    }
}
