using Microsoft.AspNetCore.Mvc;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Services;

namespace SimpleInsuranceApp.Controllers;

[ApiController]
[Route("policies")]
public class PoliciesController(PolicyService policies) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create(CreatePolicyRequest request)
    {
        var policy = policies.Create(request);
        return CreatedAtAction(nameof(Get), new { id = policy.Id }, policy);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<PolicyResponse> Activate(Guid id) => policies.Activate(id);

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PolicyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PolicyResponse> Get(Guid id) => policies.Get(id);
}
