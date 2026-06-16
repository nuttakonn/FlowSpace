using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using System.Text.Json;

namespace FlowSpace.Application.Boards.Commands.UpdateWhiteboard;

public record UpdateWhiteboardCommand(Guid BoardId, Dictionary<string, object> Records) : ICommand;

public class UpdateWhiteboardCommandHandler : ICommandHandler<UpdateWhiteboardCommand>
{
    private readonly INodeRepository _nodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWhiteboardCommandHandler(
        INodeRepository nodeRepository,
        IUnitOfWork unitOfWork)
    {
        _nodeRepository = nodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateWhiteboardCommand command, CancellationToken cancellationToken)
    {
        // We use a dedicated Metadata field in the Node to store the original tldraw ID.
        // This allows us to handle 'shape:', 'asset:', 'page:', etc. without collision or complex mapping.
        
        foreach (var entry in command.Records)
        {
            var recordId = entry.Key;
            var recordData = entry.Value;
            
            // Deterministic Guid based on recordId for stable mapping
            var nodeId = StringToGuid(recordId);

            if (recordData == null)
            {
                var existing = await _nodeRepository.GetByIdAsync(nodeId, cancellationToken);
                if (existing != null) _nodeRepository.Delete(existing);
            }
            else
            {
                var json = JsonSerializer.Serialize(recordData);
                var existing = await _nodeRepository.GetByIdAsync(nodeId, cancellationToken);
                
                if (existing == null)
                {
                    // For whiteboards, coordinates are inside metadata. We set 0,0 for simplicity in the table.
                    var node = Node.Create(nodeId, command.BoardId, "WhiteboardRecord", 0, 0);
                    node.SetMetadata(json);
                    _nodeRepository.Add(node);
                }
                else
                {
                    existing.SetMetadata(json);
                    _nodeRepository.Update(existing);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Guid StringToGuid(string value)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] data = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(data);
    }
}
