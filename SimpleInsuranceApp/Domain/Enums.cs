namespace SimpleInsuranceApp.Domain;

public enum ProductType
{
    Auto,
    Property,
    Travel,
}

public enum PolicyStatus
{
    Draft,
    Active,
}

public enum ClaimStatus
{
    New,
    Approved,
    Rejected,
}

public enum ClaimDecision
{
    Approve,
    Reject,
}

