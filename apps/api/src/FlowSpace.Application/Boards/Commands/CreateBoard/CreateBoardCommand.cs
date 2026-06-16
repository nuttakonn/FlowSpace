using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Boards.Commands.CreateBoard;

public record CreateBoardCommand(Guid WorkspaceId, string Name, string Type, Guid CreatorId) : ICommand<BoardResponse>;

public class CreateBoardCommandHandler : ICommandHandler<CreateBoardCommand, BoardResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBoardCommandHandler(
        IBoardRepository boardRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BoardResponse>> Handle(CreateBoardCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null)
        {
            return Result.Failure<BoardResponse>(new Error("Workspace.NotFound", "Workspace not found."));
        }

        var board = Board.Create(Guid.NewGuid(), command.WorkspaceId, command.Name, command.Type, command.CreatorId);

        _boardRepository.Add(board);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BoardResponse(board.Id, board.WorkspaceId, board.Name, board.Type, board.CreatedAt);
    }
}
