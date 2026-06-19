using FlowSpace.Application.Common.Abstractions.Authentication;
using Microsoft.Extensions.Configuration;

namespace FlowSpace.Infrastructure.Authentication;

public class InviteCodeService : IInviteCodeService
{
    private readonly string _validCode;

    public InviteCodeService(IConfiguration configuration)
    {
        // Read from env var INVITE_CODE, fallback to config key "InviteCode"
        _validCode = configuration["INVITE_CODE"]
            ?? configuration["InviteCode"]
            ?? throw new InvalidOperationException("INVITE_CODE is not configured.");
    }

    public bool IsValid(string inviteCode)
        => string.Equals(_validCode, inviteCode, StringComparison.Ordinal);
}
