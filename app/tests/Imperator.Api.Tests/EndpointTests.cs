using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Imperator.Core;

namespace Imperator.Api.Tests;

public class EndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string ContentPath = ContentSet.FindDefaultPath(AppContext.BaseDirectory);

    [Fact]
    public async Task ContentServesCompiledJsonWithVersion()
    {
        var response = await factory.CreateClient().GetAsync("/content", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ContentSet.LoadFile(ContentPath).Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        Assert.NotNull(response.Headers.ETag);
        Assert.Equal(await File.ReadAllBytesAsync(ContentPath, TestContext.Current.CancellationToken),
            await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ContentIsNotModifiedWhenETagMatches()
    {
        var client = factory.CreateClient();
        var first = await client.GetAsync("/content", TestContext.Current.CancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "/content");
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        var second = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task ContentIsServedWhenETagIsStale()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/content");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"0.0.0-stale\""));
        var response = await factory.CreateClient().SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReportsUnhealthyWithoutDatabase()
    {
        var response = await factory.CreateClient().GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("checks").GetProperty("database").GetString());
        Assert.Equal(ContentSet.LoadFile(ContentPath).Core.Version, body.RootElement.GetProperty("contentVersion").GetString());
    }
}
