using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid Id) : ICommand;

public class DeleteBoardCommandHandler : ICommandHandler<DeleteBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.Id, cancellationToken);
        if (board is null)
        {
            return Result.Failure(new Error("Board.NotFound", "Board not found."));
        }

        _boardRepository.Delete(board);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
