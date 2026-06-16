using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Infrastructure.Persistence;
using FlowSpace.Domain.Repositories;
using FlowSpace.Infrastructure.Persistence.Repositories;
using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Application.Common.Abstractions.AI;
using FlowSpace.Infrastructure.Authentication;
using FlowSpace.Infrastructure.Authorization;
using FlowSpace.Infrastructure.AI;
using FlowSpace.Infrastructure.AI.Prompts;
using FlowSpace.Infrastructure.Common;

using Microsoft.Extensions.Hosting;
using FlowSpace.Application.Common.Abstractions.Interop;
using FlowSpace.Infrastructure.Interop;

namespace FlowSpace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddAuth(configuration);
        services.AddPersistence(configuration);
        services.AddInterop(configuration);
        
        return services;
    }

    private static IServiceCollection AddInterop(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IExportQueue, ExportQueue>();
        services.AddScoped<IExportService, PlaywrightExportService>();
        services.AddHostedService<CanvasExportWorker>();

        services.AddHttpClient<IAiService, GeminiAiService>();
        services.AddSingleton<IPromptService, PromptService>();

        return services;
    }

    private static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.AddHttpContextAccessor();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        var redisUrl = configuration["REDIS_URL"] ?? configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrEmpty(redisUrl))
        {
            var redisOptions = RedisConfiguration.NormalizeConnectionString(redisUrl);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<INodeRepository, NodeRepository>();
        services.AddScoped<IEdgeRepository, EdgeRepository>();
        services.AddScoped<IBoardVersionRepository, BoardVersionRepository>();
        services.AddScoped<ICanvasSnapshotRepository, CanvasSnapshotRepository>();
        services.AddScoped<IAiGenerationHistoryRepository, AiGenerationHistoryRepository>();
        services.AddScoped<IBoardTemplateRepository, BoardTemplateRepository>();
        services.AddScoped<IBoardShareLinkRepository, BoardShareLinkRepository>();
        
        return services;
    }
}
