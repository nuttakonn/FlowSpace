using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FlowSpace.Application.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : ICommand<AuthenticationResponse>;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Result.Failure<AuthenticationResponse>(new Error("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        user.AddRefreshToken(refreshTokenHash, DateTime.UtcNow.AddDays(7));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(user.Id, user.Email, user.DisplayName, accessToken, refreshToken);
    }
}
