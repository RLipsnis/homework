namespace SimpleInsuranceApp.Infrastructure;

public abstract class DomainException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }

    public abstract string Title { get; }
}

public class ValidationException(string message) : DomainException(message)
{
    public override int StatusCode => StatusCodes.Status400BadRequest;

    public override string Title => "Validation failed";
}

public class NotFoundException(string message) : DomainException(message)
{
    public override int StatusCode => StatusCodes.Status404NotFound;

    public override string Title => "Resource not found";
}

public class ConflictException(string message) : DomainException(message)
{
    public override int StatusCode => StatusCodes.Status409Conflict;

    public override string Title => "Business rule conflict";
}
