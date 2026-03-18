using ExpertWaffle.Tickets.Domain;

namespace ExpertWaffle.Tickets.Ai;

public interface IAiDecisionService
{
    Task<AiTicketProposal> ProposeTicketAsync(string input, CancellationToken cancellationToken);
    Task<AiExecutionDecision> DecideNextActionAsync(Ticket ticket, string executionSummary, CancellationToken cancellationToken);
}
