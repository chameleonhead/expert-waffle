namespace ExpertWaffle.Tickets.Domain;

public sealed class Ticket
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public string? AssignedTo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public string? TaskType { get; private set; }
    public string? PayloadJson { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetryCount { get; private set; }

    private Ticket()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Ticket(
        Guid id,
        string title,
        string description,
        TicketPriority priority,
        DateTimeOffset createdAt,
        string? taskType,
        string? payloadJson,
        DateTimeOffset? scheduledAt,
        int maxRetryCount)
    {
        Id = id;
        Title = title;
        Description = description;
        Priority = priority;
        Status = TicketStatus.Open;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        TaskType = taskType;
        PayloadJson = payloadJson;
        ScheduledAt = scheduledAt;
        MaxRetryCount = maxRetryCount;
    }

    public void MarkReady(DateTimeOffset now)
    {
        EnsureCanTransitionTo(TicketStatus.Ready);
        Status = TicketStatus.Ready;
        UpdatedAt = now;
    }

    public void Start(DateTimeOffset now)
    {
        EnsureCanTransitionTo(TicketStatus.Running);
        Status = TicketStatus.Running;
        UpdatedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        EnsureCanTransitionTo(TicketStatus.Completed);
        Status = TicketStatus.Completed;
        UpdatedAt = now;
    }

    public void Fail(DateTimeOffset now)
    {
        EnsureCanTransitionTo(TicketStatus.Failed);
        Status = TicketStatus.Failed;
        RetryCount++;
        UpdatedAt = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        EnsureCanTransitionTo(TicketStatus.Cancelled);
        Status = TicketStatus.Cancelled;
        UpdatedAt = now;
    }

    public bool CanRetry() => RetryCount < MaxRetryCount;

    private void EnsureCanTransitionTo(TicketStatus next)
    {
        var valid = TicketStatusRules.CanTransition(Status, next);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"Invalid ticket status transition: {Status} -> {next}");
        }
    }
}
