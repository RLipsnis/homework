namespace SimpleInsuranceApp.Domain;

public class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string FullName { get; init; }
}
