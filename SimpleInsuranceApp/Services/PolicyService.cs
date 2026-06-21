using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;
using SimpleInsuranceApp.Infrastructure;
using SimpleInsuranceApp.Storage;

namespace SimpleInsuranceApp.Services;

public class PolicyService(InMemoryStore store, CustomerService customers)
{
    public PolicyResponse Create(CreatePolicyRequest request)
    {
        // Referenced customer must exist (404).
        _ = customers.Find(request.CustomerId);

        if (!Enum.IsDefined(request.ProductType))
        {
            throw new ValidationException("ProductType is invalid.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new ValidationException("EndDate must be after StartDate.");
        }

        if (request.Premium < 0)
        {
            throw new ValidationException("Premium must be zero or greater.");
        }

        var policy = new Policy
        {
            CustomerId = request.CustomerId,
            ProductType = request.ProductType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Premium = request.Premium,
        };

        store.Policies[policy.Id] = policy;

        return Map(policy);
    }

    public PolicyResponse Activate(Guid id)
    {
        // Lock so the overlap check and the status write happen atomically.
        lock (store.SyncRoot)
        {
            var policy = Find(id);

            if (policy.Status != PolicyStatus.Draft)
            {
                throw new ConflictException("Only draft policies can be activated.");
            }

            var hasOverlap = store.Policies.Values.Any(other =>
                other.Id != policy.Id &&
                other.CustomerId == policy.CustomerId &&
                other.ProductType == policy.ProductType &&
                other.Status == PolicyStatus.Active &&
                other.OverlapsWith(policy));

            if (hasOverlap)
            {
                throw new ConflictException(
                    "An active policy of the same product type already overlaps this period.");
            }

            policy.Status = PolicyStatus.Active;

            return Map(policy);
        }
    }

    public PolicyResponse Get(Guid id) => Map(Find(id));

    internal Policy Find(Guid id)
    {
        if (!store.Policies.TryGetValue(id, out var policy))
        {
            throw new NotFoundException($"Policy '{id}' was not found.");
        }

        return policy;
    }

    private static PolicyResponse Map(Policy policy) => new(
        policy.Id,
        policy.CustomerId,
        policy.ProductType,
        policy.StartDate,
        policy.EndDate,
        policy.Premium,
        policy.Status);
}
