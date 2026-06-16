using FluentValidation;
using FlowSpace.Contracts.Interop;

namespace FlowSpace.Application.Interop.Commands.ImportBoard;

public class ImportBoardCommandValidator : AbstractValidator<ImportBoardCommand>
{
    private const int MaxContentSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedFormats = { "flowspace", "drawio" };

    public ImportBoardCommandValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        
        RuleFor(x => x.Format)
            .NotEmpty()
            .Must(f => AllowedFormats.Contains(f.ToLower()))
            .WithMessage($"Supported formats are: {string.Join(", ", AllowedFormats)}");

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(MaxContentSize)
            .WithMessage("File content exceeds the maximum allowed size (5MB).");
    }
}
