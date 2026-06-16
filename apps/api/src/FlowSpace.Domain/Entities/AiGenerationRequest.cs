namespace FlowSpace.Domain.Entities;

public class AiGenerationRequest
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Prompt { get; private set; } = string.Empty;
    public string DiagramType { get; private set; } = string.Empty;
    public string ResultJson { get; private set; } = "{}";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AiGenerationRequest() { }

    public static AiGenerationRequest Create(Guid boardId, Guid userId, string prompt, string diagramType, string resultJson)
    {
        return new AiGenerationRequest
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Prompt = prompt,
            DiagramType = diagramType,
            ResultJson = resultJson
        };
    }
}
