using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Entities;
using FlowSpace.Infrastructure.Authorization;
using FlowSpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FlowSpace.Benchmarks;

[MemoryDiagnoser]
public class PermissionBenchmark
{
    private ApplicationDbContext _dbContext = null!;
    private IDistributedCache _cache = null!;
    private PermissionService _permissionService = null!;
    
    private Guid _userId;
    private Guid _workspaceId;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        
        var memoryCacheOptions = Options.Create(new MemoryDistributedCacheOptions());
        _cache = new MemoryDistributedCache(memoryCacheOptions);
        
        _permissionService = new PermissionService(_dbContext, _cache);

        _userId = Guid.NewGuid();
        _workspaceId = Guid.NewGuid();

        var workspace = Workspace.Create(_workspaceId, "Benchmark Workspace", _userId);
        _dbContext.Workspaces.Add(workspace);
        _dbContext.SaveChanges();
    }

    [Benchmark(Baseline = true)]
    public async Task<bool> FirstCall_DbLookup()
    {
        // Clear cache to simulate DB lookup
        await _cache.RemoveAsync($"permission:user:{_userId}:workspace:{_workspaceId}");
        return await _permissionService.HasPermissionAsync(_userId, Permissions.WorkspaceUpdate, _workspaceId);
    }

    [Benchmark]
    public async Task<bool> SubsequentCall_CacheHit()
    {
        // Cache is already populated from previous runs
        return await _permissionService.HasPermissionAsync(_userId, Permissions.WorkspaceUpdate, _workspaceId);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<PermissionBenchmark>();
    }
}
