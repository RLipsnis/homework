using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;
using SimpleInsuranceApp.Infrastructure;
using SimpleInsuranceApp.Storage;

namespace SimpleInsuranceApp.Services;

public class ClaimService(InMemoryStore store, PolicyService policies)
{
    public ClaimResponse Create(CreateClaimRequest request)
    {
        // Referenced policy must exist (404).
        var policy = policies.Find(request.PolicyId);

        if (request.AmountRequested <= 0)
        {
            throw new ValidationException("AmountRequested must be greater than zero.");
        }

        if (policy.Status != PolicyStatus.Active)
        {
            throw new ConflictException("Claims can only be created for active policies.");
        }

        if (request.IncidentDate < policy.StartDate || request.IncidentDate > policy.EndDate)
        {
            throw new ConflictException(
                "IncidentDate must fall within the policy period (inclusive).");
        }

        var claim = new Claim
        {
            PolicyId = policy.Id,
            IncidentDate = request.IncidentDate,
            AmountRequested = request.AmountRequested,
        };

        store.Claims[claim.Id] = claim;

        return Map(claim);
    }

    public ClaimResponse Decide(Guid id, DecideClaimRequest request)
    {
        // Lock so the status check and write happen atomically.
        lock (store.SyncRoot)
        {
            var claim = Find(id);

            if (claim.Status != ClaimStatus.New)
            {
                throw new ConflictException("Only new claims can be decided.");
            }

            if (!Enum.IsDefined(request.Decision))
            {
                throw new ValidationException("Decision is invalid.");
            }

            if (request.Decision == ClaimDecision.Reject)
            {
                if (string.IsNullOrWhiteSpace(request.DecisionReason))
                {
                    throw new ValidationException(
                        "DecisionReason is required when rejecting a claim.");
                }

                claim.Status = ClaimStatus.Rejected;
                claim.DecisionReason = request.DecisionReason.Trim();
            }
            else
            {
                claim.Status = ClaimStatus.Approved;
                claim.DecisionReason = null;
            }

            return Map(claim);
        }
    }

    public ClaimResponse Get(Guid id) => Map(Find(id));

    private Claim Find(Guid id)
    {
        if (!store.Claims.TryGetValue(id, out var claim))
        {
            throw new NotFoundException($"Claim '{id}' was not found.");
        }

        return claim;
    }

    private static ClaimResponse Map(Claim claim) => new(
        claim.Id,
        claim.PolicyId,
        claim.IncidentDate,
        claim.AmountRequested,
        claim.Status,
        claim.DecisionReason);
}
