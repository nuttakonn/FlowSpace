using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace FlowSpace.Application.Authentication.Commands.Logout;

public record LogoutCommand(string RefreshToken) : ICommand;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var providedTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(command.RefreshToken)));
        var user = await _userRepository.GetByRefreshTokenHashAsync(providedTokenHash, cancellationToken);

        if (user is not null)
        {
            user.RevokeRefreshToken(providedTokenHash);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
