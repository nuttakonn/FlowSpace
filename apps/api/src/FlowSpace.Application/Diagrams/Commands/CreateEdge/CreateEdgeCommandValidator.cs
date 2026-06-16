using FluentValidation;

namespace FlowSpace.Application.Diagrams.Commands.CreateEdge;

public class CreateEdgeCommandValidator : AbstractValidator<CreateEdgeCommand>
{
    public CreateEdgeCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.SourceNodeId).NotEmpty();
        RuleFor(x => x.TargetNodeId).NotEmpty();
    }
}
