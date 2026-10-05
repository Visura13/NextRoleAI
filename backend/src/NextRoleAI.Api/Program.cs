using System.Text.Json.Serialization;
using NextRoleAI.Infrastructure;
using NextRoleAI.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler();
}

app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.Services.InitialiseDatabaseAsync(
    app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"),
    app.Environment.IsDevelopment() &&
        app.Configuration.GetValue<bool>("SeedData:Enabled"));

await app.RunAsync();

public partial class Program;
