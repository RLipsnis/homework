using System.Text.Json.Serialization;
using SimpleInsuranceApp.Infrastructure;
using SimpleInsuranceApp.Services;
using SimpleInsuranceApp.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger UI is mounted at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Consistent error contract: all DomainExceptions become ProblemDetails responses.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// In-memory storage (singleton) + per-request domain services.
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<PolicyService>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();
app.MapGet("/swagger", () => Results.Content(SwaggerUi.Html, "text/html"))
    .ExcludeFromDescription();

app.MapControllers();

app.Run();

