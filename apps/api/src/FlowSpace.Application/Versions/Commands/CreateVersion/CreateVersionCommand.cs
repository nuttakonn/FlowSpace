using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Versions;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Versions.Commands.CreateVersion;

public record CreateVersionCommand(
    Guid BoardId,
    Guid UserId,
    string? Name,
    string? Description,
    string NodesData,
    string EdgesData,
    byte[] YjsState,
    VersionType Type) : ICommand<BoardVersionResponse>;

public class CreateVersionCommandHandler : ICommandHandler<CreateVersionCommand, BoardVersionResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardVersionRepository _versionRepository;
    private readonly ICanvasSnapshotRepository _snapshotRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVersionCommandHandler(
        IBoardRepository boardRepository,
        IBoardVersionRepository versionRepository,
        ICanvasSnapshotRepository snapshotRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _versionRepository = versionRepository;
        _snapshotRepository = snapshotRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BoardVersionResponse>> Handle(CreateVersionCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null)
        {
            return Result.Failure<BoardVersionResponse>(new Error("Board.NotFound", "Board not found."));
        }

        var snapshot = CanvasSnapshot.Create(command.BoardId, command.NodesData, command.EdgesData, command.YjsState);
        _snapshotRepository.Add(snapshot);

        var version = BoardVersion.Create(
            command.BoardId, 
            command.UserId, 
            snapshot.Id, 
            command.Type, 
            command.Name, 
            command.Description);

        _versionRepository.Add(version);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        return new BoardVersionResponse(
            version.Id,
            version.BoardId,
            version.Name,
            version.Description,
            version.CreatedAt,
            version.CreatedBy,
            user?.DisplayName ?? "Unknown",
            (int)version.Type);
    }
}
