using System.Text.Json;
using Imperator.Api;
using Imperator.Api.Data;
using Imperator.Core;
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

app.Run();

// Exposed for WebApplicationFactory in Imperator.Api.Tests.
public partial class Program;
