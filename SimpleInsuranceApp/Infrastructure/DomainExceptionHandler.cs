using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SimpleInsuranceApp.Infrastructure;

// Translates DomainExceptions into ProblemDetails responses so every business failure shares one consistent error contract.
public class DomainExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domain)
        {
            return false;
        }

        httpContext.Response.StatusCode = domain.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = domain,
            ProblemDetails = new ProblemDetails
            {
                Status = domain.StatusCode,
                Title = domain.Title,
                Detail = domain.Message,
            },
        });
    }
}
