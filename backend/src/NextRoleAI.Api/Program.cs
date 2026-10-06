using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using NextRoleAI.Api.Configuration;
using NextRoleAI.Infrastructure;
using NextRoleAI.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")) &&
    !string.IsNullOrWhiteSpace(databaseUrl))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        PostgresUrlConnectionString.Convert(databaseUrl);
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NextRoleAI API",
        Version = "v1",
        Description = "Shared recruitment API for the NextRoleAI React and Flutter clients."
    });
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the access token returned by a NextRoleAI authentication endpoint."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
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

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NextRoleAI API v1");
    options.DocumentTitle = "NextRoleAI API";
});

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
