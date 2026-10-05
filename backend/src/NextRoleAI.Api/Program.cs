using NextRoleAI.Infrastructure;
using NextRoleAI.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.Services.InitialiseDatabaseAsync(
    app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"));

await app.RunAsync();

public partial class Program;
