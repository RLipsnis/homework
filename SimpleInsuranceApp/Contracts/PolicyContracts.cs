using SimpleInsuranceApp.Domain;

namespace SimpleInsuranceApp.Contracts;

public record CreatePolicyRequest(
    Guid CustomerId,
    ProductType ProductType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Premium);

public record PolicyResponse(
    Guid Id,
    Guid CustomerId,
    ProductType ProductType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Premium,
    PolicyStatus Status);
