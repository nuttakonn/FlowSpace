using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Sharing;
using FlowSpace.Domain.Authorization;
using FlowSpace.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace FlowSpace.Application.Sharing.Commands.CreateShareLink;

public record CreateShareLinkCommand(Guid BoardId, BoardRole Role, DateTime? ExpiresAt = null) : ICommand<ShareLinkResponse>;

public class CreateShareLinkCommandHandler : ICommandHandler<CreateShareLinkCommand, ShareLinkResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardShareLinkRepository _shareLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _frontendUrl;

    public CreateShareLinkCommandHandler(
        IBoardRepository boardRepository,
        IBoardShareLinkRepository shareLinkRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _boardRepository = boardRepository;
        _shareLinkRepository = shareLinkRepository;
        _unitOfWork = unitOfWork;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
    }

    public async Task<Result<ShareLinkResponse>> Handle(CreateShareLinkCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null) return Result.Failure<ShareLinkResponse>(new Error("Board.NotFound", "Board not found."));

        var link = board.CreateShareLink(command.Role, command.ExpiresAt);
        _shareLinkRepository.Add(link);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShareLinkResponse(
            link.Id,
            link.Token,
            (int)link.Role,
            link.ExpiresAt,
            link.IsRevoked,
            link.CreatedAt,
            $"{_frontendUrl}/shared/{link.Token}"
        );
    }
}
