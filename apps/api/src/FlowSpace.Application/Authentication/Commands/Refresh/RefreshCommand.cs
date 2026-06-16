using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FlowSpace.Application.Authentication.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : ICommand<AuthenticationResponse>;

public class RefreshCommandHandler : ICommandHandler<RefreshCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponse>> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var providedTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(command.RefreshToken)));
        var user = await _userRepository.GetByRefreshTokenHashAsync(providedTokenHash, cancellationToken);

        if (user is null || !user.HasValidRefreshToken(providedTokenHash))
        {
            return Result.Failure<AuthenticationResponse>(new Error("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newRefreshToken)));

        user.RevokeRefreshToken(providedTokenHash);
        user.AddRefreshToken(newRefreshTokenHash, DateTime.UtcNow.AddDays(7));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(user.Id, user.Email, user.DisplayName, accessToken, newRefreshToken);
    }
}
