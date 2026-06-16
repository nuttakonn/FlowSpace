using FluentValidation;

namespace FlowSpace.Application.Boards.Commands.CreateBoard;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    private static readonly string[] AllowedTypes = { "Whiteboard", "Flowchart", "Mindmap", "Wireframe" };

    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().Must(type => AllowedTypes.Contains(type))
            .WithMessage($"Type must be one of: {string.Join(", ", AllowedTypes)}");
        RuleFor(x => x.CreatorId).NotEmpty();
    }
}
