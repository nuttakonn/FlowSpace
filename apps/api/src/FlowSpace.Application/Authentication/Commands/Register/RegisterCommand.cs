using FlowSpace.Application.Common.Abstractions.Authentication;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Authentication;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FlowSpace.Application.Authentication.Commands.Register;

public record RegisterCommand(string Email, string Password, string DisplayName, string? InviteCode = null) : ICommand<AuthenticationResponse>;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IInviteCodeService _inviteCodeService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IInviteCodeService inviteCodeService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _inviteCodeService = inviteCodeService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        // Guest shadow accounts bypass invite code validation
        bool isGuestEmail = command.Email.StartsWith("guest-") && command.Email.EndsWith("@flowspace.local");

        if (!isGuestEmail && (string.IsNullOrEmpty(command.InviteCode) || !_inviteCodeService.IsValid(command.InviteCode)))
        {
            return Result.Failure<AuthenticationResponse>(new Error("Auth.InvalidInviteCode", "Invalid invite code."));
        }

        if (await _userRepository.GetByEmailAsync(command.Email, cancellationToken) is not null)
        {
            return Result.Failure<AuthenticationResponse>(new Error("Auth.EmailAlreadyExists", "Email is already in use."));
        }

        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var user = User.Create(Guid.NewGuid(), command.Email, passwordHash, command.DisplayName);

        _userRepository.Add(user);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        
        user.AddRefreshToken(refreshTokenHash, DateTime.UtcNow.AddDays(7));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponse(user.Id, user.Email, user.DisplayName, accessToken, refreshToken);
    }
}
