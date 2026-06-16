namespace FlowSpace.Domain.Entities;

public class BoardTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public string BoardType { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public User? Creator { get; private set; }
    public string ContentJson { get; private set; } = "{}";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private BoardTemplate() { }

    public static BoardTemplate Create(
        string name, 
        string description, 
        string boardType, 
        string contentJson, 
        bool isSystem = false, 
        Guid? createdBy = null)
    {
        return new BoardTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            BoardType = boardType,
            ContentJson = contentJson,
            IsSystem = isSystem,
            CreatedBy = createdBy
        };
    }
}
