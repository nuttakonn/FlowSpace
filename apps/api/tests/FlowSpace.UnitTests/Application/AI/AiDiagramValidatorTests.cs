using FlowSpace.Application.AI.Validators;
using FlowSpace.Contracts.AI;
using FluentAssertions;
using FluentValidation.Results;

namespace FlowSpace.UnitTests.Application.AI;

public class AiDiagramValidatorTests
{
    private readonly AiDiagramResponseValidator _responseValidator;

    public AiDiagramValidatorTests()
    {
        _responseValidator = new AiDiagramResponseValidator();
    }

    [Fact]
    public void ResponseValidator_ShouldFail_WhenEdgeSourceIsMissing()
    {
        // Arrange
        var response = new AiDiagramResponse(
            "1.0.0",
            new List<AiNodeResponse> 
            { 
                new AiNodeResponse("node_1", "Rectangle", null, new AiNodeData("Start"), "{}") 
            },
            new List<AiEdgeResponse> 
            { 
                new AiEdgeResponse("edge_1", "node_1", "node_missing", "Connection") 
            }
        );

        // Act
        ValidationResult result = _responseValidator.Validate(response);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("invalid target node node_missing"));
    }

    [Fact]
    public void ResponseValidator_ShouldSucceed_WhenGraphIsConsistent()
    {
        // Arrange
        var response = new AiDiagramResponse(
            "1.0.0",
            new List<AiNodeResponse> 
            { 
                new AiNodeResponse("n1", "Rectangle", null, new AiNodeData("A"), "{}"),
                new AiNodeResponse("n2", "Circle", null, new AiNodeData("B"), "{}")
            },
            new List<AiEdgeResponse> 
            { 
                new AiEdgeResponse("e1", "n1", "n2", null) 
            }
        );

        // Act
        ValidationResult result = _responseValidator.Validate(response);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
