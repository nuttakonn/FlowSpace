using FluentValidation;
using FlowSpace.Contracts.AI;

namespace FlowSpace.Application.AI.Validators;

public class AiDiagramRequestValidator : AbstractValidator<AiDiagramRequest>
{
    public AiDiagramRequestValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Prompt must be between 1 and 2000 characters.");

        RuleFor(x => x.DiagramType)
            .IsInEnum();

        RuleFor(x => x.Constraints)
            .ChildRules(c => {
                c.RuleFor(x => x!.MaxNodes)
                    .InclusiveBetween(1, 100)
                    .When(x => x != null);
            });
    }
}

public class AiDiagramResponseValidator : AbstractValidator<AiDiagramResponse>
{
    public AiDiagramResponseValidator()
    {
        RuleFor(x => x.SchemaVersion)
            .NotEmpty()
            .Matches(@"^\d+\.\d+\.\d+$")
            .WithMessage("Schema version must follow semver (e.g., 1.0.0).");

        RuleFor(x => x.Nodes)
            .NotEmpty()
            .WithMessage("AI must return at least one node.");

        RuleForEach(x => x.Nodes).ChildRules(node => {
            node.RuleFor(n => n.Id).NotEmpty();
            node.RuleFor(n => n.Type).NotEmpty();
            node.RuleFor(n => n.Data.Label).NotEmpty().MaximumLength(200);
        });

        RuleFor(x => x.Edges).Custom((edges, context) => {
            var response = context.InstanceToValidate;
            foreach (var edge in edges)
            {
                if (string.IsNullOrEmpty(edge.Id)) 
                    context.AddFailure($"Edge with index {edges.IndexOf(edge)} is missing an ID.");
                
                if (!response.Nodes.Any(n => n.Id == edge.Source))
                {
                    context.AddFailure("Edges", $"Edge {edge.Id} has invalid source node {edge.Source}.");
                }
                if (!response.Nodes.Any(n => n.Id == edge.Target))
                {
                    context.AddFailure("Edges", $"Edge {edge.Id} has invalid target node {edge.Target}.");
                }
            }
        });
    }
}
