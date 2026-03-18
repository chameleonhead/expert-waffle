using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Ai;

public sealed class AiTicketProposal
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public TicketPriority Priority { get; init; }
    public string TaskType { get; init; } = "";
    public string PayloadJson { get; init; } = "";
    public DateTimeOffset? ScheduledAt { get; init; }
}

public sealed class AiExecutionDecision
{
    public bool Retry { get; init; }
    public bool Escalate { get; init; }
    public string Reason { get; init; } = "";
    public TicketPriority? NewPriority { get; init; }
}
