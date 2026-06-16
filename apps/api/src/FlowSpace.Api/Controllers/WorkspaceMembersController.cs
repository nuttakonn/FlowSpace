using FlowSpace.Application.Workspaces.Commands.ChangeMemberRole;
using FlowSpace.Application.Workspaces.Commands.InviteMember;
using FlowSpace.Application.Workspaces.Commands.RemoveMember;
using FlowSpace.Application.Workspaces.Queries.GetWorkspaceMembers;
using FlowSpace.Contracts.Workspaces;
using FlowSpace.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/workspaces/{workspaceId:guid}/members")]
[Authorize]
public class WorkspaceMembersController : ApiController
{
    private readonly ISender _sender;

    public WorkspaceMembersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.NodeRead)]
    public async Task<IActionResult> GetMembers(Guid workspaceId)
    {
        var query = new GetWorkspaceMembersQuery(workspaceId);
        var result = await _sender.Send(query);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.WorkspaceUpdate)]
    public async Task<IActionResult> InviteMember(Guid workspaceId, InviteMemberRequest request)
    {
        var command = new InviteMemberCommand(workspaceId, request.Email, (Domain.Authorization.WorkspaceRole)request.Role);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpPut("{userId:guid}/role")]
    [Authorize(Policy = Permissions.WorkspaceUpdate)]
    public async Task<IActionResult> ChangeRole(Guid workspaceId, Guid userId, ChangeRoleRequest request)
    {
        var command = new ChangeMemberRoleCommand(workspaceId, userId, (Domain.Authorization.WorkspaceRole)request.Role);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = Permissions.WorkspaceUpdate)]
    public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid userId)
    {
        var command = new RemoveMemberCommand(workspaceId, userId);
        var result = await _sender.Send(command);

        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
