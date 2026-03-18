namespace ExpertWaffle.Tickets.Domain;

public sealed class TicketExecution
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public TicketExecutionResult Result { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorDetail { get; private set; }

    private TicketExecution() { }

    public TicketExecution(Guid id, Guid ticketId, DateTimeOffset startedAt)
    {
        Id = id;
        TicketId = ticketId;
        StartedAt = startedAt;
        Result = TicketExecutionResult.Running;
    }

    public void Complete(DateTimeOffset endedAt)
    {
        EndedAt = endedAt;
        Result = TicketExecutionResult.Succeeded;
    }

    public void Fail(DateTimeOffset endedAt, string? errorCode, string? errorMessage, string? errorDetail)
    {
        EndedAt = endedAt;
        Result = TicketExecutionResult.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorDetail = errorDetail;
    }
}
