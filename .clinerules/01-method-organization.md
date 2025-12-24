# Method/Function Organization Rules

When generating, refactoring, or editing code files (especially classes or modules):

- Group all **public** methods/functions (e.g., `public`, `export`, or API-exposed) together at the **top** of the file or class.
- Alphabetize public methods by name (case-sensitive, A-Z).
- Leave a clear separation (e.g., a comment block or blank lines) between public and private sections.
- Group all **private/internal** methods/functions (e.g., `private`, `protected`, `_prefixed`, or helper functions) together at the **bottom**.
- Alphabetize private methods by name (case-sensitive, A-Z).

## Examples

### Preferred (C# class)
```csharp
public class MyService
{
    // PUBLIC METHODS (alphabetized)
    public void AddItem() { ... }
    public void DeleteItem() { ... }
    public async Task GetItems() { ... }

    // PRIVATE METHODS (alphabetized)
    private void LogError() { ... }
    private async Task ValidateInput() { ... }
}