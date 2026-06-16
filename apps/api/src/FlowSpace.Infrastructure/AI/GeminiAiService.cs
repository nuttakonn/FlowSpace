using System.Net.Http.Json;
using System.Text.Json;
using FlowSpace.Application.Common.Abstractions.AI;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.AI;
using FlowSpace.Infrastructure.AI.Prompts;
using FlowSpace.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace FlowSpace.Infrastructure.AI;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly IPromptService _promptService;
    private readonly IBoardTemplateRepository _templateRepository;
    private readonly string _apiKey;
    private const string ModelId = "gemini-1.5-flash";

    private static readonly Meter AiMeter = new("FlowSpace.AI", "1.0.0");
    private static readonly Counter<int> GenerationAttempts = AiMeter.CreateCounter<int>("ai.generation.attempts");
    private static readonly Counter<int> GenerationFailures = AiMeter.CreateCounter<int>("ai.generation.failures");

    public GeminiAiService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<GeminiAiService> logger,
        IPromptService promptService,
        IBoardTemplateRepository templateRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _promptService = promptService;
        _templateRepository = templateRepository;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing.");
    }

    public async Task<Result<AiDiagramResponse>> GenerateDiagramAsync(AiDiagramRequest request, CancellationToken cancellationToken = default)
    {
        GenerationAttempts.Add(1, new KeyValuePair<string, object?>("type", request.DiagramType.ToString()));
        
        var systemInstruction = AiPromptBuilder.BuildSystemInstruction();
        
        var variables = new Dictionary<string, string>
        {
            { "MaxNodes", request.Constraints?.MaxNodes?.ToString() ?? "20" },
            { "Language", request.Constraints?.Language ?? "English" },
            { "Prompt", request.Prompt },
            { "TemplateContext", "" },
            { "ExistingNodes", JsonSerializer.Serialize(request.Context?.ExistingNodes ?? new()) },
            { "ExistingEdges", JsonSerializer.Serialize(request.Context?.ExistingEdges ?? new()) },
            { "RefinementCommand", request.RefinementCommand?.ToString() ?? "" }
        };

        if (request.Context?.TemplateId.HasValue == true)
        {
            var template = await _templateRepository.GetByIdAsync(request.Context.TemplateId.Value, cancellationToken);
            if (template != null)
            {
                variables["TemplateContext"] = $"Use this template structure as a base: {template.ContentJson}";
            }
        }

        var templateName = request.RefinementCommand.HasValue 
            ? (request.RefinementCommand.Value == AiRefinementCommand.ExplainDiagram ? "Explain" : "Refinement")
            : request.DiagramType.ToString();

        var userPrompt = await _promptService.GetPromptAsync(templateName, variables);

        var isTextResponse = request.RefinementCommand == AiRefinementCommand.ExplainDiagram;

        var geminiRequest = new
        {
            system_instruction = new { parts = new[] { new { text = systemInstruction } } },
            contents = new[]
            {
                new { parts = new[] { new { text = userPrompt } } }
            },
            generationConfig = new
            {
                responseMimeType = isTextResponse ? "text/plain" : "application/json"
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={_apiKey}",
                geminiRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API error: {StatusCode}, {Body}", response.StatusCode, errorBody);
                GenerationFailures.Add(1, new KeyValuePair<string, object?>("reason", "api_error"));
                return Result.Failure<AiDiagramResponse>(new Error("AI.ApiError", "Failed to communicate with Gemini API."));
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            var jsonText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrEmpty(jsonText))
            {
                GenerationFailures.Add(1, new KeyValuePair<string, object?>("reason", "empty_response"));
                return Result.Failure<AiDiagramResponse>(new Error("AI.EmptyResponse", "Gemini returned an empty response."));
            }

            if (isTextResponse)
            {
                // For ExplainDiagram, we return the text in a special "explanation" node or just as a metadata field
                return new AiDiagramResponse("1.0.0", new List<AiNodeResponse> { 
                    new AiNodeResponse("explanation", "StickyNote", new AiNodePosition(0, 0), new AiNodeData("Diagram Explanation", jsonText), "{}") 
                }, new());
            }

            var parsedResult = AiResponseParser.Parse(jsonText);
            if (parsedResult.IsFailure)
            {
                GenerationFailures.Add(1, new KeyValuePair<string, object?>("reason", "parse_error"));
            }
            return parsedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during AI generation.");
            GenerationFailures.Add(1, new KeyValuePair<string, object?>("reason", "exception"));
            return Result.Failure<AiDiagramResponse>(new Error("AI.Exception", "An unexpected error occurred during AI generation."));
        }
    }

    private record GeminiResponse(List<Candidate> Candidates);
    private record Candidate(Content Content);
    private record Content(List<Part> Parts);
    private record Part(string Text);
}
