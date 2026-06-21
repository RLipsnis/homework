using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;

namespace SimpleInsuranceApp.Tests;

// Spins up the real API in-memory (WebApplicationFactory) and exposes an HttpClient
// plus small helpers so each test reads as a short end-to-end scenario. A new instance
// per test means a fresh, isolated in-memory store.
public class TestApi : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public HttpClient Client { get; }

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public TestApi() => Client = _factory.CreateClient();

    public async Task<Guid> CreateCustomerAsync(string fullName = "Jane Doe")
    {
        var response = await Client.PostAsJsonAsync(
            "/customers", new CreateCustomerRequest(fullName), Json);
        response.EnsureSuccessStatusCode();
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(Json);
        return customer!.Id;
    }

    public async Task<HttpResponseMessage> CreatePolicyAsync(
        Guid customerId,
        ProductType productType = ProductType.Auto,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal premium = 100m)
    {
        var request = new CreatePolicyRequest(
            customerId,
            productType,
            startDate ?? new DateOnly(2026, 1, 1),
            endDate ?? new DateOnly(2026, 12, 31),
            premium);
        return await Client.PostAsJsonAsync("/policies", request, Json);
    }

    public async Task<PolicyResponse> CreateActivePolicyAsync(
        Guid customerId,
        ProductType productType = ProductType.Auto,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var create = await CreatePolicyAsync(customerId, productType, startDate, endDate);
        create.EnsureSuccessStatusCode();
        var policy = await create.Content.ReadFromJsonAsync<PolicyResponse>(Json);

        var activate = await Client.PostAsync($"/policies/{policy!.Id}/activate", null);
        activate.EnsureSuccessStatusCode();
        return (await activate.Content.ReadFromJsonAsync<PolicyResponse>(Json))!;
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
