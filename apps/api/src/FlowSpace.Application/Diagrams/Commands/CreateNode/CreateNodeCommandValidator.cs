using FluentValidation;

namespace FlowSpace.Application.Diagrams.Commands.CreateNode;

public class CreateNodeCommandValidator : AbstractValidator<CreateNodeCommand>
{
    public CreateNodeCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
    }
}
