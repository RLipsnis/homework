using System.Net;
using System.Net.Http.Json;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;
using Xunit;

namespace SimpleInsuranceApp.Tests;

public class PolicyTests
{
    [Fact]
    public async Task Activate_draft_policy_succeeds()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();
        var create = await api.CreatePolicyAsync(customerId);
        var policy = await create.Content.ReadFromJsonAsync<PolicyResponse>(TestApi.Json);

        var activate = await api.Client.PostAsync($"/policies/{policy!.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);
        var activated = await activate.Content.ReadFromJsonAsync<PolicyResponse>(TestApi.Json);
        Assert.Equal(PolicyStatus.Active, activated!.Status);
    }

    [Fact]
    public async Task Activate_already_active_policy_returns_409()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();
        var policy = await api.CreateActivePolicyAsync(customerId);

        var secondActivate = await api.Client.PostAsync($"/policies/{policy.Id}/activate", null);

        Assert.Equal(HttpStatusCode.Conflict, secondActivate.StatusCode);
    }

    [Fact]
    public async Task Activating_overlapping_active_policy_same_product_returns_409()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();

        // First Auto policy: Jan -> Jun, activated.
        await api.CreateActivePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30));

        // Second Auto policy overlapping (Mar -> Sep): create is fine, activation conflicts.
        var second = await api.CreatePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 3, 1), new DateOnly(2026, 9, 30));
        var secondPolicy = await second.Content.ReadFromJsonAsync<PolicyResponse>(TestApi.Json);

        var activate = await api.Client.PostAsync($"/policies/{secondPolicy!.Id}/activate", null);

        Assert.Equal(HttpStatusCode.Conflict, activate.StatusCode);
    }

    [Fact]
    public async Task Activating_overlapping_policy_of_different_product_is_allowed()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();

        await api.CreateActivePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        // Same dates but a different product type -> no conflict.
        var property = await api.CreatePolicyAsync(
            customerId, ProductType.Property,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        var propertyPolicy = await property.Content.ReadFromJsonAsync<PolicyResponse>(TestApi.Json);

        var activate = await api.Client.PostAsync($"/policies/{propertyPolicy!.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);
    }

    [Fact]
    public async Task Creating_policy_with_end_date_not_after_start_returns_400()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();

        var response = await api.CreatePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Getting_unknown_policy_returns_404()
    {
        using var api = new TestApi();

        var response = await api.Client.GetAsync($"/policies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
