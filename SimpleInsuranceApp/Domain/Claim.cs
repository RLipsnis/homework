namespace SimpleInsuranceApp.Domain;

public class Claim
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid PolicyId { get; init; }

    public required DateOnly IncidentDate { get; init; }

    public required decimal AmountRequested { get; init; }

    public ClaimStatus Status { get; set; } = ClaimStatus.New;

    public string? DecisionReason { get; set; }
}
