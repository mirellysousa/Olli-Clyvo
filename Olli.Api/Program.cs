using System.Threading.RateLimiting;
using HealthChecks.UI.Client;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using IdempotentAPI.MinimalAPI;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Olli.Api;
using Olli.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region Database
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "InMemory";
if (builder.Environment.IsEnvironment("Testing"))
{
    databaseProvider = "InMemory";
}

var isOracleProvider = databaseProvider.Equals("Oracle", StringComparison.OrdinalIgnoreCase);
var applyMigrationsOnStartup = builder.Configuration.GetValue<bool?>("ApplyMigrationsOnStartup") ?? false;
var seedDatabase = builder.Configuration.GetValue<bool?>("SeedDatabase") ?? !isOracleProvider;
if (builder.Environment.IsEnvironment("Testing"))
{
    seedDatabase = true;
}

var oracleConnectionString = builder.Configuration.GetConnectionString("FiapOracle");
if (isOracleProvider && string.IsNullOrWhiteSpace(oracleConnectionString))
{
    throw new InvalidOperationException("Connection string FiapOracle nao configurada.");
}

builder.Services.AddDbContext<OlliDb>(
    options =>
    {
        if (isOracleProvider)
        {
            options.UseOracle(oracleConnectionString);
            return;
        }

        options.UseInMemoryDatabase("OlliDb");
    });
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
#endregion

#region Idempotency
builder.Services.AddIdempotentAPI();
builder.Services.AddIdempotentMinimalAPI(new IdempotencyOptions());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddIdempotentAPIUsingDistributedCache();
#endregion

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(1)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain";
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync(
            "Muitas requisicoes. Por favor, tente novamente mais tarde.",
            cancellationToken);
    };
});

var healthChecks = builder.Services.AddHealthChecks();

if (isOracleProvider)
{
    healthChecks.AddOracle(
        connectionString: oracleConnectionString!,
        name: "oracle-fiap",
        failureStatus: HealthStatus.Degraded,
        tags: ["Db", "Oracle"],
        healthQuery: "SELECT 1 FROM DUAL",
        timeout: TimeSpan.FromSeconds(300)
    );
}
else
{
    healthChecks.AddCheck("in-memory-db", () =>
        HealthCheckResult.Healthy("Banco em memoria ativo para desenvolvimento local."));
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHealthChecksUI(options =>
    {
        options.SetEvaluationTimeInSeconds(150);
        options.MaximumHistoryEntriesPerEndpoint(5);
        options.SetApiMaxActiveRequests(1);
        options.AddHealthCheckEndpoint("api", "/health");
    }).AddInMemoryStorage();
}

#region OpenAPI / Swagger / Scalar
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo()
        {
            Title = "API Olli - Clyvo VET",
            Version = "1.0.0",
            Description = """
                          API para acompanhamento preventivo da saude pet.

                          A Olli conecta tutor, pet e clinica veterinaria em uma jornada continua.
                          A IA deste MVP gera recomendacoes preventivas, mas nao realiza diagnostico definitivo.

                          Boa vizinhanca:

                          Recomendamos no maximo 10 requisicoes por segundo.
                          Caso haja exagero de uso, a API retornara status code 429 (Too Many Requests).
                          """,
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/license/mit/")
            },
            Contact = new OpenApiContact()
            {
                Email = "contato@olli.pet",
                Name = "Equipe Olli",
                Url = new Uri("https://www.fiap.com.br")
            }
        };
        return Task.CompletedTask;
    });
});
#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OlliDb>();
    if (db.Database.IsRelational() && applyMigrationsOnStartup)
    {
        await db.Database.MigrateAsync();
    }

    if (seedDatabase)
    {
        await OlliDbSeed.SeedAsync(db);
    }
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (!app.Environment.IsEnvironment("Testing"))
{
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";
    });
}

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.RegistrarRotasOlli();

public partial class Program { }
