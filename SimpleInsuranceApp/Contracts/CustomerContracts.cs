namespace SimpleInsuranceApp.Contracts;

public record CreateCustomerRequest(string FullName);

public record CustomerResponse(Guid Id, string FullName);
