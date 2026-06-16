using System.Reflection;
using System.Text.RegularExpressions;

namespace FlowSpace.Infrastructure.AI.Prompts;

public interface IPromptService
{
    Task<string> GetPromptAsync(string templateName, IDictionary<string, string> variables, string version = "v1");
}

public class PromptService : IPromptService
{
    private readonly string _promptsPath;

    public PromptService(string? promptsPath = null)
    {
        if (!string.IsNullOrEmpty(promptsPath))
        {
            _promptsPath = promptsPath;
            return;
        }

        // Discovery logic
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        
        var pathsToTry = new[]
        {
            Path.Combine(assemblyLocation, "AI", "Prompts"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "FlowSpace.Infrastructure", "AI", "Prompts"),
            Path.Combine(Directory.GetCurrentDirectory(), "apps", "api", "src", "FlowSpace.Infrastructure", "AI", "Prompts"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "src", "FlowSpace.Infrastructure", "AI", "Prompts") // For tests
        };

        _promptsPath = pathsToTry.FirstOrDefault(Directory.Exists) 
                      ?? throw new DirectoryNotFoundException("AI Prompts directory not found.");
    }

    public async Task<string> GetPromptAsync(string templateName, IDictionary<string, string> variables, string version = "v1")
    {
        var fileName = $"{templateName}.{version}.md";
        var filePath = Path.Combine(_promptsPath, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Prompt template {fileName} not found.");
        }

        var content = await File.ReadAllTextAsync(filePath);

        // Interpolate variables: {{VariableName}}
        foreach (var variable in variables)
        {
            content = content.Replace($"{{{{{variable.Key}}}}}", variable.Value);
        }

        return content;
    }
}
