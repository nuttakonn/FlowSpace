using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Boards.Commands.UpdateBoard;

public record UpdateBoardCommand(Guid Id, string Name) : ICommand<BoardResponse>;

public class UpdateBoardCommandHandler : ICommandHandler<UpdateBoardCommand, BoardResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BoardResponse>> Handle(UpdateBoardCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.Id, cancellationToken);
        if (board is null)
        {
            return Result.Failure<BoardResponse>(new Error("Board.NotFound", "Board not found."));
        }

        board.Update(command.Name);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BoardResponse(board.Id, board.WorkspaceId, board.Name, board.Type, board.CreatedAt);
    }
}
