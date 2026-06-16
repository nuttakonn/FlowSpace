using FluentValidation;

namespace FlowSpace.Application.Diagrams.Queries.GetElements;

public class GetElementsQueryValidator : AbstractValidator<GetElementsQuery>
{
    public GetElementsQueryValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
    }
}
