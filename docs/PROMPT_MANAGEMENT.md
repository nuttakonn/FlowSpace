# AI Prompt Management System

## Overview
The AI Prompt Management System decouples large, complex LLM instructions from the application code. It allows for versioning, template reuse, and variable interpolation, ensuring that diagram generation logic remains clean and maintainable.

---

## 1. Directory Structure
Prompts are stored as Markdown files in:
`apps/api/src/FlowSpace.Infrastructure/AI/Prompts/`

Naming Convention:
`{TemplateName}.{Version}.md` (e.g., `Flowchart.v1.md`)

---

## 2. Template Variables
Templates support variable interpolation using the `{{VariableName}}` syntax. 

**Common Variables:**
- `{{Prompt}}`: The user's natural language description.
- `{{MaxNodes}}`: The maximum number of nodes allowed in the response.
- `{{Language}}`: The target language for labels and descriptions.

---

## 3. Supported Templates

### Flowchart
- **File**: `Flowchart.v1.md`
- **Focus**: Process logic, decision branches (Diamonds), and action steps (Rectangles).

### Mindmap
- **File**: `Mindmap.v1.md`
- **Focus**: Hierarchical deconstruction of a topic. Uses `StickyNote` nodes.

### Architecture
- **File**: `Architecture.v1.md`
- **Focus**: Cloud infrastructure, microservices, and data flow. Uses icons and protocol labels.

### User Journey
- **File**: `UserJourney.v1.md`
- **Focus**: Horizontal sequencing of user touchpoints and emotional states.

---

## 4. Usage in Code

Inject `IPromptService` and resolve a template:

```csharp
var variables = new Dictionary<string, string>
{
    { "MaxNodes", "20" },
    { "Prompt", "Create a checkout flow." }
};

string finalPrompt = await _promptService.GetPromptAsync("Flowchart", variables, "v1");
```

---

## 5. Versioning Strategy
To update a prompt without breaking existing boards:
1. Create a new file: `Flowchart.v2.md`.
2. Update the `AiProvider` or `CommandHandler` to request version `"v2"`.
3. Old logic can continue using `"v1"` until migrated.
