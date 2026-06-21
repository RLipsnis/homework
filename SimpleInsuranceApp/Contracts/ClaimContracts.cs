using SimpleInsuranceApp.Domain;

namespace SimpleInsuranceApp.Contracts;

public record CreateClaimRequest(
    Guid PolicyId,
    DateOnly IncidentDate,
    decimal AmountRequested);

public record DecideClaimRequest(
    ClaimDecision Decision,
    string? DecisionReason);

public record ClaimResponse(
    Guid Id,
    Guid PolicyId,
    DateOnly IncidentDate,
    decimal AmountRequested,
    ClaimStatus Status,
    string? DecisionReason);
