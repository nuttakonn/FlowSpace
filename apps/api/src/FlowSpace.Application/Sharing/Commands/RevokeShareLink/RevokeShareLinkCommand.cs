using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Sharing.Commands.RevokeShareLink;

public record RevokeShareLinkCommand(Guid LinkId) : ICommand;

public class RevokeShareLinkCommandHandler : ICommandHandler<RevokeShareLinkCommand>
{
    private readonly IBoardShareLinkRepository _shareLinkRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeShareLinkCommandHandler(IBoardShareLinkRepository shareLinkRepository, IUnitOfWork unitOfWork)
    {
        _shareLinkRepository = shareLinkRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeShareLinkCommand command, CancellationToken cancellationToken)
    {
        var link = await _shareLinkRepository.GetByIdAsync(command.LinkId, cancellationToken);
        if (link is null) return Result.Failure(new Error("ShareLink.NotFound", "Share link not found."));

        link.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
