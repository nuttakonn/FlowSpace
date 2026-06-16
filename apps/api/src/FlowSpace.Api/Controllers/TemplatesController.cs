using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Repositories;
using FlowSpace.Contracts.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowSpace.Api.Controllers;

[Route("api/v1/templates")]
[Authorize]
public class TemplatesController : ApiController
{
    private readonly IBoardTemplateRepository _templateRepository;

    public TemplatesController(IBoardTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates([FromQuery] string? type, [FromQuery] bool? isSystem)
    {
        var templates = await _templateRepository.ListAsync(type, isSystem);
        
        var response = templates.Select(t => new BoardTemplateResponse(
            t.Id,
            t.Name,
            t.Description,
            t.ThumbnailUrl,
            t.BoardType,
            t.IsSystem,
            t.CreatedAt)).ToList();

        return Ok(response);
    }
}
