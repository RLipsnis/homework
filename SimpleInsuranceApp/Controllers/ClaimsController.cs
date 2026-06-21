using Microsoft.AspNetCore.Mvc;
using SimpleInsuranceApp.Contracts;
using SimpleInsuranceApp.Services;

namespace SimpleInsuranceApp.Controllers;

[ApiController]
[Route("claims")]
public class ClaimsController(ClaimService claims) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Create(CreateClaimRequest request)
    {
        var claim = claims.Create(request);
        return CreatedAtAction(nameof(Get), new { id = claim.Id }, claim);
    }

    [HttpPost("{id:guid}/decide")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<ClaimResponse> Decide(Guid id, DecideClaimRequest request) =>
        claims.Decide(id, request);

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ClaimResponse> Get(Guid id) => claims.Get(id);
}
