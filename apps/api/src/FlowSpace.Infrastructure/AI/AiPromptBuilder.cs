using FlowSpace.Contracts.AI;
using System.Text;

namespace FlowSpace.Infrastructure.AI;

public static class AiPromptBuilder
{
    public static string BuildSystemInstruction()
    {
        return @"You are an Expert System Architect and Visual Designer. 
Your task is to generate a structured diagram schema in JSON format based on the user's prompt.
You must strictly follow the provided schema version 1.0.0.

Rules:
1. Output MUST be valid JSON.
2. Every node must have a unique ID (e.g., 'node_1', 'node_2').
3. Every edge source and target must match a valid node ID.
4. Use standard Flowchart shapes: Rectangle, Circle, Diamond.
5. If the diagram is a Mindmap, use a tree structure.
6. If it's a System Architecture, identify Frontend, Backend, and Database layers.
7. Return only the JSON object.

Output Schema:
{
  ""schemaVersion"": ""1.0.0"",
  ""nodes"": [
    { ""id"": ""string"", ""type"": ""Rectangle|Circle|Diamond|StickyNote"", ""data"": { ""label"": ""string"", ""description"": ""string"" }, ""metadata"": ""{}"" }
  ],
  ""edges"": [
    { ""id"": ""string"", ""source"": ""string"", ""target"": ""string"", ""label"": ""string"" }
  ],
  ""layoutHint"": ""vertical|horizontal""
}";
    }

    public static string BuildUserPrompt(AiDiagramRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Create a {request.DiagramType} based on this description: {request.Prompt}");
        
        if (request.Constraints?.MaxNodes.HasValue == true)
        {
            sb.AppendLine($"Constraint: Maximum {request.Constraints.MaxNodes} nodes.");
        }
        
        if (!string.IsNullOrEmpty(request.Constraints?.Language))
        {
            sb.AppendLine($"Language: {request.Constraints.Language}");
        }

        return sb.ToString();
    }
}
