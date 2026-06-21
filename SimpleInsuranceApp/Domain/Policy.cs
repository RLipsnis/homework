namespace SimpleInsuranceApp.Domain;

public class Policy
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid CustomerId { get; init; }

    public required ProductType ProductType { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public required decimal Premium { get; init; }

    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    // Two policy periods overlap when each one starts on or before the other ends.
    // Boundaries are inclusive, so touching periods are considered overlapping.
    public bool OverlapsWith(Policy other) =>
        StartDate <= other.EndDate && other.StartDate <= EndDate;
}
