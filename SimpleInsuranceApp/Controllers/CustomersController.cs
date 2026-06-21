using Microsoft.AspNetCore.Mvc;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Services;

namespace SimpleInsuranceApp.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController(CustomerService customers) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(CreateCustomerRequest request)
    {
        var customer = customers.Create(request);
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CustomerResponse>> GetAll() => Ok(customers.GetAll());

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CustomerResponse> Get(Guid id) => customers.Get(id);
}
