using System.Net;
using System.Net.Http.Json;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Domain;
using Xunit;

namespace SimpleInsuranceApp.Tests;

public class ClaimTests
{
    [Fact]
    public async Task Creating_claim_for_draft_policy_returns_409()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();

        // Draft policy (created, not activated).
        var create = await api.CreatePolicyAsync(customerId);
        var policy = await create.Content.ReadFromJsonAsync<PolicyResponse>(TestApi.Json);

        var response = await api.Client.PostAsJsonAsync(
            "/claims",
            new CreateClaimRequest(policy!.Id, new DateOnly(2026, 6, 1), 500m),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Creating_claim_with_incident_outside_policy_period_returns_409()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();
        var policy = await api.CreateActivePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30));

        // Incident in December is outside the Jan-Jun policy period.
        var response = await api.Client.PostAsJsonAsync(
            "/claims",
            new CreateClaimRequest(policy.Id, new DateOnly(2026, 12, 1), 500m),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Creating_claim_for_active_policy_within_period_succeeds()
    {
        using var api = new TestApi();
        var customerId = await api.CreateCustomerAsync();
        var policy = await api.CreateActivePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        var response = await api.Client.PostAsJsonAsync(
            "/claims",
            new CreateClaimRequest(policy.Id, new DateOnly(2026, 6, 15), 500m),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(TestApi.Json);
        Assert.Equal(ClaimStatus.New, claim!.Status);
    }

    [Fact]
    public async Task Rejecting_claim_without_reason_returns_400()
    {
        using var api = new TestApi();
        var claimId = await CreateNewClaimAsync(api);

        var response = await api.Client.PostAsJsonAsync(
            $"/claims/{claimId}/decide",
            new DecideClaimRequest(ClaimDecision.Reject, DecisionReason: null),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rejecting_claim_with_reason_succeeds()
    {
        using var api = new TestApi();
        var claimId = await CreateNewClaimAsync(api);

        var response = await api.Client.PostAsJsonAsync(
            $"/claims/{claimId}/decide",
            new DecideClaimRequest(ClaimDecision.Reject, "Insufficient evidence"),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claim = await response.Content.ReadFromJsonAsync<ClaimResponse>(TestApi.Json);
        Assert.Equal(ClaimStatus.Rejected, claim!.Status);
        Assert.Equal("Insufficient evidence", claim.DecisionReason);
    }

    [Fact]
    public async Task Deciding_already_decided_claim_returns_409()
    {
        using var api = new TestApi();
        var claimId = await CreateNewClaimAsync(api);

        var approve = await api.Client.PostAsJsonAsync(
            $"/claims/{claimId}/decide",
            new DecideClaimRequest(ClaimDecision.Approve, DecisionReason: null),
            TestApi.Json);
        approve.EnsureSuccessStatusCode();

        var secondDecision = await api.Client.PostAsJsonAsync(
            $"/claims/{claimId}/decide",
            new DecideClaimRequest(ClaimDecision.Reject, "Changed my mind"),
            TestApi.Json);

        Assert.Equal(HttpStatusCode.Conflict, secondDecision.StatusCode);
    }

    private static async Task<Guid> CreateNewClaimAsync(TestApi api)
    {
        var customerId = await api.CreateCustomerAsync();
        var policy = await api.CreateActivePolicyAsync(
            customerId, ProductType.Auto,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        var create = await api.Client.PostAsJsonAsync(
            "/claims",
            new CreateClaimRequest(policy.Id, new DateOnly(2026, 6, 15), 500m),
            TestApi.Json);
        create.EnsureSuccessStatusCode();
        var claim = await create.Content.ReadFromJsonAsync<ClaimResponse>(TestApi.Json);
        return claim!.Id;
    }
}
