using Racha.Application;
using Racha.Infrastructure;
using Racha.Web.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRachaWebApi(builder.Configuration, "Racha API");

var app = builder.Build();

// This API owns the database schema: in Development it applies migrations (and seeds demo data) on startup.
// The Auth API doesn't, so the two never race to migrate the same database.
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync(
        seedDemoData: app.Configuration.GetValue("Database:SeedDemoData", defaultValue: true));
}

app.UseRachaWebApi();

app.MapControllers();

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API with WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program
{
}
