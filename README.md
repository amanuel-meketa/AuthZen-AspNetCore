# AuthZen-AspNetCore

Lightweight fine-grained authorization library for ASP.NET Core using the AuthZen protocol.

AuthZen.AspNetCore provides a simple, protocol-driven way to protect your ASP.NET Core microservices by delegating access decisions to a central authorization service.

---

## Installation

using AuthZen.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register AuthZen authorization
builder.Services.AddAuthZenAuthorization(options =>
{
    // Full URL to your central AuthZen authorization service
    options.FullUrl = "https://auth-service.local/api/access/check";
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();


Controller Usage with Attribute

Protect your endpoints using the AuthZenAuthorize attribute:

using AuthZen.AspNetCore.Filters;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TemplateController(TemplateService templateService)
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

    [HttpPost]
    [AuthZenAuthorize("create")]
    public async Task<IActionResult> CreateTemplate([FromBody] TemplateDto dto)
    {
        await _templateService.CreateAsync(dto);
        return Ok();
    }
}


Notes:

"view" and "create" represent the relation to check.

By default, the userId is taken from the logged-in user (HttpContext.User).

Optional UserId Override

You can override the userId if needed:

[AuthZenAuthorize("view", userId: "123")]


If omitted, AuthZen uses the current logged-in user automatically.

Configuration via appsettings.json

Store the AuthZen service URL in your configuration:

{
  "AuthZen": {
    "FullUrl": "https://auth-service.local/api/access/check"
  }
}


And register it in Program.cs:

builder.Services.AddAuthZenAuthorization(options =>
{
    options.FullUrl = builder.Configuration["AuthZen:FullUrl"];
});


Notes:

FullUrl should point to your central AuthZen authorization service.

The library automatically handles DI for AuthorizationServiceHttp.

