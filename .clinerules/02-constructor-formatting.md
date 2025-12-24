# C# Constructor and Method Parameter Formatting Rules

When generating, refactoring, or editing C# code (especially constructors and methods with dependency injection):

- If a constructor or method has **more than one parameter**, format the parameters as follows:
  - The first parameter stays on the same line as the opening parenthesis.
  - Every subsequent parameter must be on its own line.
  - The comma is placed at the **beginning** of the line (leading comma style).
  - Each continued parameter line is indented exactly **4 spaces** from the start of the opening parenthesis line.

- Always include a blank line before the opening brace `{` if the parameter list spans multiple lines.

## Preferred Example (Constructor)

```csharp
public InfluxDBWriter(
    ILogger<InfluxDBWriter> logger
    , IConfiguration configuration
    , SomeOtherService service)
{
    // body
}

public BackfillController(
    IBackgroundTaskQueue queue
    , IServiceProvider services
    , ILogger<BackfillController> logger
    , SensorPushClient client
    , IConfiguration configuration)
{
    _queue = queue;
    _services = services;
    _logger = logger;
    _client = client;
    _configuration = configuration;
}
```

Bad Examples:
```
// Avoid - trailing comma, no leading comma
public BadExample(ILogger<BadExample> logger,
                  IConfiguration configuration)
```

```
// Avoid - all on one line or inconsistent indentation
public BadExample(ILogger<BadExample> logger, IConfiguration configuration)

```