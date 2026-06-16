namespace FlowSpace.Application.Common.Abstractions.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, Guid? resourceId = null);
    Task<bool> HasPermissionAsync(string token, string permission, Guid boardId);
}
