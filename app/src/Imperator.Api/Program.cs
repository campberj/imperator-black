using System.Text.Json;
using Imperator.Api;
using Imperator.Api.Data;
using Imperator.Core;
using Imperator.Core.Land;
using Imperator.Core.Layouts;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Content is validated at startup so the API never serves a broken content.json.
var contentPath = builder.Configuration["Content:Path"] ?? ContentSet.FindDefaultPath(builder.Environment.ContentRootPath);
var content = ContentFile.Load(contentPath);
builder.Services.AddSingleton(content);

builder.Services.AddDbContext<ImperatorDb>(o => o
    .UseNpgsql(builder.Configuration.GetConnectionString("Imperator"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddHealthChecks().AddDbContextCheck<ImperatorDb>("database");

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = (ctx, report) => ctx.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        contentVersion = content.Version,
        checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()),
    }),
});

app.MapGet("/content", (HttpContext ctx) =>
{
    ctx.Response.Headers.ETag = content.ETag;
    ctx.Response.Headers["Imperator-Content-Version"] = content.Version;
    var etag = EntityTagHeaderValue.Parse(content.ETag);
    if (ctx.Request.GetTypedHeaders().IfNoneMatch.Any(t => t.Equals(EntityTagHeaderValue.Any) || t.Compare(etag, useStrongComparison: false)))
        return Results.StatusCode(StatusCodes.Status304NotModified);
    return Results.Bytes(content.Json, "application/json");
});

// Generated boards use the scenario format's names (snake_case, nulls left out), as in docs/scenarios/.
var deckJson = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
};

app.MapGet("/decks/{vessel}", (HttpContext ctx, string vessel, int? seed) =>
{
    var vessels = content.Set.Layouts.Vessels.Select(v => v.Id).ToList();
    if (!vessels.Contains(vessel))
        return Results.Problem($"Unknown vessel '{vessel}'. Vessels: {string.Join(", ", vessels)}.", statusCode: StatusCodes.Status404NotFound);
    if (seed is null)
        return Results.Problem($"A seed is required, e.g. /decks/{vessel}?seed=7. The same seed and content version always give the same deck.",
            statusCode: StatusCodes.Status400BadRequest);
    ctx.Response.Headers["Imperator-Content-Version"] = content.Version;
    return Results.Json(DeckGenerator.Generate(content.Set, vessel, seed.Value).Deck, deckJson);
});

app.MapGet("/land/{environment}", (HttpContext ctx, string environment, int? seed, string? size) =>
{
    var environments = content.Set.Terrain.Environments.Select(e => e.Id).ToList();
    if (!environments.Contains(environment))
        return Results.Problem($"Unknown environment '{environment}'. Environments: {string.Join(", ", environments)}.", statusCode: StatusCodes.Status404NotFound);
    size ??= "small";
    if (!LandTableGenerator.BoardSides.ContainsKey(size))
        return Results.Problem($"Unknown size '{size}'. Sizes: {string.Join(", ", LandTableGenerator.BoardSides.Keys)}.", statusCode: StatusCodes.Status400BadRequest);
    if (seed is null)
        return Results.Problem($"A seed is required, e.g. /land/{environment}?size={size}&seed=7. The same seed and content version always give the same table.",
            statusCode: StatusCodes.Status400BadRequest);
    ctx.Response.Headers["Imperator-Content-Version"] = content.Version;
    return Results.Json(LandTableGenerator.Generate(content.Set, environment, size, seed.Value), deckJson);
});

app.Run();

// Exposed for WebApplicationFactory in Imperator.Api.Tests.
public partial class Program;
