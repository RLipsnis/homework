
using System.Text.Json.Serialization;
using InsuranceCore.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
// Swagger UI is mounted at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();
app.MapGet("/swagger", () => Results.Content(SwaggerUi.Html, "text/html"))
    .ExcludeFromDescription();

app.MapControllers();

app.Run();

