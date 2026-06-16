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

// Enable Forwarded Headers for Cloud Proxies (Render/Koyeb)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsProduction() || app.Environment.EnvironmentName == "Cloud")
{
    app.UseCors("CloudCorsPolicy");
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("GlobalLimit");
// Apply stricter limit to Auth controller specifically in your implementation if needed, 
// or keep it global. For safety, we apply GlobalLimit to all.

app.MapHub<CollaborationHub>("/hubs/collaboration");
app.MapHealthChecks("/health");

// Apply Migrations if configured
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
