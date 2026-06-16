using Microsoft.AspNetCore.Authorization;
using FlowSpace.Application.Common.Abstractions.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FlowSpace.Infrastructure.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        // Try to get resource ID from route (e.g., workspaceId, boardId)
        var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
        Guid? resourceId = null;

        if (routeData?.Values.TryGetValue("workspaceId", out var wId) == true && Guid.TryParse(wId?.ToString(), out var parsedWId))
        {
            resourceId = parsedWId;
        }
        else if (routeData?.Values.TryGetValue("boardId", out var bId) == true && Guid.TryParse(bId?.ToString(), out var parsedBId))
        {
            resourceId = parsedBId;
        }
        else if (routeData?.Values.TryGetValue("id", out var id) == true && Guid.TryParse(id?.ToString(), out var parsedId))
        {
            resourceId = parsedId;
        }

        if (await _permissionService.HasPermissionAsync(userId, requirement.Permission, resourceId))
        {
            context.Succeed(requirement);
        }
    }
}
