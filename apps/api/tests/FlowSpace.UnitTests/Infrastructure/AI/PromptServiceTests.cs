using FlowSpace.Infrastructure.AI.Prompts;
using FluentAssertions;

namespace FlowSpace.UnitTests.Infrastructure.AI;

public class PromptServiceTests
{
    private readonly PromptService _promptService;

    public PromptServiceTests()
    {
        // For tests, we provide the path relative to where the binary is (bin/Debug/net10.0)
        var path = Path.GetFullPath("../../../../../src/FlowSpace.Infrastructure/AI/Prompts");
        _promptService = new PromptService(path);
    }

    [Fact]
    public async Task GetPromptAsync_ShouldInterpolateVariables()
    {
        // Arrange
        var variables = new Dictionary<string, string>
        {
            { "MaxNodes", "10" },
            { "Language", "French" },
            { "Prompt", "Un diagramme de test" }
        };

        // Act
        var result = await _promptService.GetPromptAsync("Flowchart", variables);

        // Assert
        result.Should().Contain("Max Nodes: 10");
        result.Should().Contain("Language: French");
    }
}
