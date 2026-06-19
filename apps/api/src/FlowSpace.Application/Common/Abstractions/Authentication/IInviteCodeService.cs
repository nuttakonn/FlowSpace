namespace FlowSpace.Application.Common.Abstractions.Authentication;

public interface IInviteCodeService
{
    bool IsValid(string inviteCode);
}
