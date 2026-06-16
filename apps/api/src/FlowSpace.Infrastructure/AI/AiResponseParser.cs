using System.Text.Json;
using FlowSpace.Contracts.AI;
using FlowSpace.Application.Common.Models;

namespace FlowSpace.Infrastructure.AI;

public static class AiResponseParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Result<AiDiagramResponse> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result.Failure<AiDiagramResponse>(new Error("AI.EmptyResponse", "The AI response is empty."));
        }

        try
        {
            // Clean the JSON string (Gemini sometimes wraps JSON in markdown blocks)
            var cleanedJson = CleanMarkdownJson(json);
            
            var response = JsonSerializer.Deserialize<AiDiagramResponse>(cleanedJson, Options);
            if (response == null)
            {
                return Result.Failure<AiDiagramResponse>(new Error("AI.InvalidJson", "Failed to deserialize AI response."));
            }

            return response;
        }
        catch (JsonException ex)
        {
            return Result.Failure<AiDiagramResponse>(new Error("AI.JsonError", $"AI response is not valid JSON: {ex.Message}"));
        }
    }

    private static string CleanMarkdownJson(string json)
    {
        json = json.Trim();
        if (json.StartsWith("```json"))
        {
            json = json.Substring(7);
        }
        else if (json.StartsWith("```"))
        {
            json = json.Substring(3);
        }

        if (json.EndsWith("```"))
        {
            json = json.Substring(0, json.Length - 3);
        }

        return json.Trim();
    }
}
