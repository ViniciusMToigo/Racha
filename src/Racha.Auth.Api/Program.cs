using Racha.Application;
using Racha.Infrastructure;
using Racha.Web.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRachaWebApi(builder.Configuration, "Racha Auth API");

var app = builder.Build();

app.UseRachaWebApi();

app.MapControllers();

// Public endpoint: everything else requires a token (fallback policy).
app.MapGet("/api/health", () => TypedResults.Ok(new { Status = "Healthy" }))
    .AllowAnonymous()
    .WithTags("Health");

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API with WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program
{
}
