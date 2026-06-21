using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;
using SimpleInsuranceApp.Infrastructure;
using SimpleInsuranceApp.Storage;

namespace SimpleInsuranceApp.Services;

public class CustomerService(InMemoryStore store)
{
    public CustomerResponse Create(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ValidationException("FullName is required.");
        }

        var customer = new Customer { FullName = request.FullName.Trim() };
        store.Customers[customer.Id] = customer;

        return Map(customer);
    }

    public IReadOnlyList<CustomerResponse> GetAll() =>
        store.Customers.Values.Select(Map).ToList();

    public CustomerResponse Get(Guid id) => Map(Find(id));

    internal Customer Find(Guid id)
    {
        if (!store.Customers.TryGetValue(id, out var customer))
        {
            throw new NotFoundException($"Customer '{id}' was not found.");
        }

        return customer;
    }

    private static CustomerResponse Map(Customer customer) =>
        new(customer.Id, customer.FullName);
}
