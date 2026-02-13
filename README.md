# AuthZen-AspNetCore

Lightweight fine-grained authorization library for ASP.NET Core using the AuthZen protocol.

AuthZen.AspNetCore provides a simple, protocol-driven way to protect your ASP.NET Core microservices by delegating access decisions to a central authorization service.

---

## Table of Contents

1. [Installation](#installation)
2. [DI / Program.cs Setup](#di--programcs-setup)
3. [Controller Usage with Attribute](#controller-usage-with-attribute)
4. [Optional UserId Override](#optional-userid-override)
5. [Configuration via appsettings.json](#configuration-via-appsettingsjson)
6. [Example Microservice Flow](#example-microservice-flow)
7. [Quick Start Microservice Example](#quick-start-microservice-example)
8. [License](#license)

---

## Installation

Install the NuGet package in your microservice:

```bash
dotnet add package AuthZen.AspNetCore
```

---

## DI / Program.cs Setup

Register AuthZen in your microservice:

```csharp
using AuthZen.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register AuthZen authorization
builder.Services.AddAuthZenAuthorization(options =>
{
    // URL to your central AuthZen authorization service
    options.Url = "https://auth-service.local/api/access/check";
});

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

> **Notes:**
>
> * `Url` should point to your central AuthZen authorization service.
> * The library automatically handles DI for `AuthorizationServiceHttp`.

---

## Controller Usage with Attribute

Protect your endpoints using the `AuthZenAuthorize` attribute:

```csharp
using AuthZen.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly TestService _testService;

    public TestController(TestService testService)
    {
        _testService = testService;
    }

    [HttpGet]
    [AuthZenAuthorize("view", ResourceType , ResourceId )]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _testService.GetAllAsync();
        return Ok(templates);
    }

    [HttpPost]
    [AuthZenAuthorize("create")]
    public async Task<IActionResult> Create([FromBody] TestDto dto)
    {
        await _testService.CreateAsync(dto);
        return Ok();
    }
}
```

> **Notes:**
>
> * "view" and "create" represent the **relation** to check.
> * * "ResourceType" and "ResourceId" represent the **Resource-based authorization is a fine-grained access control mechanism that determines user permissions based on specific resource ** to check.
> * By default, the **userId is taken from the logged-in user** (`HttpContext.User`).

---

## Optional UserId Override

You can override the `userId` if needed:

```csharp
[AuthZenAuthorize("view", ResourceType , ResourceId , UserId)]
```

* If omitted, AuthZen uses the **current logged-in user** automatically.

---

## Configuration via appsettings.json

Store the AuthZen service URL in your configuration:

```json
{
  "AuthZen": {
    "Url": "https://auth-service.local/api/access/check"
  }
}
```

And register it in **Program.cs**:

```csharp
builder.Services.AddAuthZenAuthorization(options =>
{
    options.Url = builder.Configuration["AuthZen:Url"];
});
```

> **Notes:**
>
> * The library automatically handles DI for `AuthorizationServiceHttp`.
> * `Url` must point to your central AuthZen authorization service.

---

## Example Microservice Flow

1. Microservice receives HTTP request.
2. `AuthZenAuthorizeAttribute` triggers **authorization check**.
3. `AuthorizationServiceHttp` sends request to central AuthZen service.
4. AuthZen service responds `true` or `false`.
5. Attribute either **allows execution** or **returns 403 Forbidden**.

---

## Quick Start Microservice Example

Minimal microservice structure using AuthZen.AspNetCore:

```
Microservice/
│
├─ Program.cs
├─ appsettings.json
├─ Controllers/
│   └─ TestController.cs
├─ Services/
│   └─ TestService.cs
└─ Dtos/
    └─ TestDto.cs
```

**Program.cs**

```csharp
using AuthZen.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthZenAuthorization(options =>
{
    options.Url = "https://authZ-service.local/api/access/check";
});

builder.Services.AddScoped<TestService>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

**TemplateController.cs**

```csharp
using AuthZen.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TestController(TemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    [AuthZenAuthorize("view")]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _templateService.GetAllAsync();
        return Ok(templates);
    }
}
```

**TestService.cs**

```csharp
public class TestService
{
    public Task<List<TemplateDto>> GetAllAsync()
    {
        // Dummy data for testing
        return Task.FromResult(new List<TemplateDto>
        {
            new TemplateDto { Id = "1", Name = "Sample Template" }
        });
    }
}
```

**TestDto.cs**

```csharp
public class TestDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
```

---

## License

This library is **MIT licensed**. See [LICENSE](LICENSE) for details.
