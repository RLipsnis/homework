using System.Net;
using System.Net.Http.Json;
using SimpleInsuranceApp.Contracts;
using Xunit;

namespace SimpleInsuranceApp.Tests;

public class CustomerTests
{
    [Fact]
    public async Task Creating_customer_returns_201_with_id()
    {
        using var api = new TestApi();

        var response = await api.Client.PostAsJsonAsync(
            "/customers", new CreateCustomerRequest("Jane Doe"), TestApi.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(TestApi.Json);
        Assert.NotEqual(Guid.Empty, customer!.Id);
        Assert.Equal("Jane Doe", customer.FullName);
    }

    [Fact]
    public async Task Getting_unknown_customer_returns_404()
    {
        using var api = new TestApi();

        var response = await api.Client.GetAsync($"/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
