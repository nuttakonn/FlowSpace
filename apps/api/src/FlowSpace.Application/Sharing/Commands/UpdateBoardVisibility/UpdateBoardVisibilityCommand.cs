using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Sharing.Commands.UpdateBoardVisibility;

public record UpdateBoardVisibilityCommand(Guid BoardId, BoardVisibility Visibility) : ICommand;

public class UpdateBoardVisibilityCommandHandler : ICommandHandler<UpdateBoardVisibilityCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBoardVisibilityCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBoardVisibilityCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null) return Result.Failure(new Error("Board.NotFound", "Board not found."));

        board.UpdateVisibility(command.Visibility);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
