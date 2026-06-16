using FlowSpace.Domain.Entities;

namespace FlowSpace.Application.Common.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
