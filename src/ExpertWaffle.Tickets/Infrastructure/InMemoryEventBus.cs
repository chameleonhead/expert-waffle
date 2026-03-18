using ExpertWaffle.Tickets.Application;

namespace ExpertWaffle.Tickets.Infrastructure;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly List<object> _publishedEvents = new();
    private readonly object _lock = new();

    public IReadOnlyList<object> PublishedEvents
    {
        get
        {
            lock (_lock)
            {
                return _publishedEvents.AsReadOnly();
            }
        }
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        lock (_lock)
        {
            _publishedEvents.Add(@event);
        }
        return Task.CompletedTask;
    }
}
