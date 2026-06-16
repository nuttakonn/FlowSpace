using FlowSpace.Application.AI.Commands.GenerateDiagram;
using FlowSpace.Application.Common.Abstractions.AI;
using FlowSpace.Contracts.AI;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Authorization;
using FluentValidation;
using Moq;
using FluentAssertions;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Entities;

namespace FlowSpace.UnitTests.Application.AI;

public class AiGenerationTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IBoardRepository> _boardRepoMock;
    private readonly Mock<INodeRepository> _nodeRepoMock;
    private readonly Mock<IEdgeRepository> _edgeRepoMock;
    private readonly Mock<IAiGenerationHistoryRepository> _historyRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IValidator<AiDiagramResponse>> _validatorMock;
    private readonly Mock<IPermissionService> _permissionMock;
    private readonly GenerateDiagramCommandHandler _handler;

    public AiGenerationTests()
    {
        _aiServiceMock = new Mock<IAiService>();
        _boardRepoMock = new Mock<IBoardRepository>();
        _nodeRepoMock = new Mock<INodeRepository>();
        _edgeRepoMock = new Mock<IEdgeRepository>();
        _historyRepoMock = new Mock<IAiGenerationHistoryRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<AiDiagramResponse>>();
        _permissionMock = new Mock<IPermissionService>();

        _handler = new GenerateDiagramCommandHandler(
            _aiServiceMock.Object,
            _boardRepoMock.Object,
            _nodeRepoMock.Object,
            _edgeRepoMock.Object,
            _historyRepoMock.Object,
            _uowMock.Object,
            _validatorMock.Object,
            _permissionMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenAiReturnsValidDiagram()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new GenerateDiagramCommand(boardId, userId, "Simple flow", AiDiagramType.Flowchart);
        
        _permissionMock.Setup(x => x.HasPermissionAsync(userId, "Node.Create", boardId))
            .ReturnsAsync(true);

        _boardRepoMock.Setup(x => x.GetByIdAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Board.Create(boardId, Guid.NewGuid(), "Test Board", "Flowchart", Guid.NewGuid()));

        var aiResponse = new AiDiagramResponse(
            "1.0.0",
            new List<AiNodeResponse> { new AiNodeResponse("n1", "Rectangle", null, new AiNodeData("Node A"), "{}") },
            new List<AiEdgeResponse>(),
            "vertical"
        );

        _aiServiceMock.Setup(x => x.GenerateDiagramAsync(It.IsAny<AiDiagramRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(aiResponse));

        _validatorMock.Setup(v => v.ValidateAsync(aiResponse, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _nodeRepoMock.Verify(x => x.Add(It.IsAny<Node>()), Times.Once);
        _historyRepoMock.Verify(x => x.Add(It.IsAny<AiGenerationRequest>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRbacFails()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new GenerateDiagramCommand(boardId, userId, "Simple flow", AiDiagramType.Flowchart);

        _permissionMock.Setup(x => x.HasPermissionAsync(userId, "Node.Create", boardId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Forbidden");
    }
}
