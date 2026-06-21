using System.Collections.Concurrent;
using SimpleInsuranceApp.Domain;

namespace SimpleInsuranceApp.Storage;

// Registered as a singleton so all requests share the same data
public class InMemoryStore
{
    public ConcurrentDictionary<Guid, Customer> Customers { get; } = new();

}
