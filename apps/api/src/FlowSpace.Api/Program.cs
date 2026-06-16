using Serilog;
using FlowSpace.Api.Middleware;
using FlowSpace.Api.Hubs;
using FlowSpace.Application;
using FlowSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using FlowSpace.Infrastructure.Persistence;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.EntityFrameworkCore;
using FlowSpace.Infrastructure.Common;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add Configuration Providers for Secrets
builder.Configuration.AddEnvironmentVariables();
if (Directory.Exists("/run/secrets"))
{
    builder.Configuration.AddKeyPerFile(directoryPath: "/run/secrets", optional: true, reloadOnChange: true);
}

// Add Serilog
builder.Host.UseSerilog((context, loggerConfig) => 
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add OpenTelemetry
var serviceName = "FlowSpace.Api";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global limit
    options.AddFixedWindowLimiter("GlobalLimit", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 20;
    });

    // Stricter limit for Authentication
    options.AddFixedWindowLimiter("AuthLimit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
});

// Add services
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CloudCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(',') ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register SignalR
var redisUrl = builder.Configuration["REDIS_URL"] ?? builder.Configuration.GetConnectionString("RedisConnection");
var signalrBuilder = builder.Services.AddSignalR()
    .AddMessagePackProtocol();

if (!string.IsNullOrEmpty(redisUrl))
{
    var redisOptions = RedisConfiguration.NormalizeConnectionString(redisUrl);
    signalrBuilder.AddStackExchangeRedis(redisOptions);
}

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configure HSTS for Production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

var app = builder.Build();

// 1. Health Checks - Must be first to respond immediately to Render's port detection and health checks
// This bypasses all middleware (Auth, Redirection, Rate Limiting) for efficiency.
app.UseHealthChecks("/health");

// 2. Enable Forwarded Headers for Cloud Proxies (Render/Koyeb)
// This must be early to ensure correct protocol (HTTP/HTTPS) detection.
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3. HTTPS Redirection - Disabled in Cloud/Production as Render handles SSL termination
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsProduction() || app.Environment.EnvironmentName == "Cloud")
{
    app.UseCors("CloudCorsPolicy");
}
else
{
    app.UseCors("DevCorsPolicy");
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("GlobalLimit");
app.MapHub<CollaborationHub>("/hubs/collaboration");

// 4. Start the Application and then Apply Migrations
// We use RunAsync() to start the host immediately so that Render detects the open port.
// Then we run migrations in the background.
var runTask = app.RunAsync();

if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

await runTask;
