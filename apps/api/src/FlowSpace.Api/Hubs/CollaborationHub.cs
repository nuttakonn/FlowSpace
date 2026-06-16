using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FlowSpace.Domain.Repositories;
using FlowSpace.Domain.Authorization;
using System.Security.Claims;

namespace FlowSpace.Api.Hubs;

[AllowAnonymous]
public class CollaborationHub : Hub
{
    private readonly IPermissionService _permissionService;
    private readonly IBoardShareLinkRepository _shareLinkRepository;

    public CollaborationHub(
        IPermissionService permissionService,
        IBoardShareLinkRepository shareLinkRepository)
    {
        _permissionService = permissionService;
        _shareLinkRepository = shareLinkRepository;
    }

    public async Task JoinBoard(Guid boardId, string? token = null)
    {
        bool canUpdate = false;
        var userId = TryGetUserId();

        if (userId.HasValue)
        {
            canUpdate = await _permissionService.HasPermissionAsync(userId.Value, Permissions.NodeUpdate, boardId);
        }

        if (!canUpdate && !string.IsNullOrEmpty(token))
        {
            var link = await _shareLinkRepository.GetByTokenAsync(token);
            if (link != null && link.BoardId == boardId && link.IsValid())
            {
                canUpdate = link.Role == BoardRole.Editor || link.Role == BoardRole.Owner;
            }
        }

        if (canUpdate || await IsViewer(boardId, userId, token))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());
            Context.Items["CanUpdate"] = canUpdate;
            Context.Items["BoardId"] = boardId;
            Context.Items["Token"] = token;
            await Clients.Caller.SendAsync("OnJoined", boardId);
        }
        else
        {
            throw new HubException("Forbidden: You do not have permission to join this board.");
        }
    }

    public async Task LeaveBoard(Guid boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId.ToString());
    }

    public async Task UpdateCanvas(Guid boardId, byte[] update)
    {
        if (Context.Items.TryGetValue("CanUpdate", out var canUpdate) && (bool)canUpdate!)
        {
            await Clients.OthersInGroup(boardId.ToString()).SendAsync("OnUpdate", update);
        }
        else
        {
            throw new HubException("Forbidden: Read-only access.");
        }
    }

    public async Task SendAwareness(Guid boardId, byte[] awarenessState)
    {
        await Clients.OthersInGroup(boardId.ToString()).SendAsync("OnAwareness", awarenessState);
    }

    public async Task SyncState(Guid boardId, byte[] stateVector)
    {
        await Clients.OthersInGroup(boardId.ToString()).SendAsync("OnSyncRequest", stateVector);
    }

    private async Task<bool> IsViewer(Guid boardId, Guid? userId, string? token)
    {
        if (userId.HasValue && await _permissionService.HasPermissionAsync(userId.Value, Permissions.NodeRead, boardId))
            return true;

        if (!string.IsNullOrEmpty(token))
        {
            var link = await _shareLinkRepository.GetByTokenAsync(token);
            return link != null && link.BoardId == boardId && link.IsValid();
        }

        return false;
    }

    private Guid? TryGetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
