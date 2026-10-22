using System.Net;
using System.Text.Json;
using Imperator.Core;
using Imperator.Core.Land;

namespace Imperator.Api.Tests;

public class LandEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly ContentSet Content = ContentSet.LoadDefault(AppContext.BaseDirectory);

    public static TheoryData<string> EnvironmentIds() => new(Content.Terrain.Environments.Select(e => e.Id));

    private async Task<(HttpResponseMessage Response, JsonElement Body)> Get(string url)
    {
        var response = await factory.CreateClient().GetAsync(url, TestContext.Current.CancellationToken);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (response, JsonDocument.Parse(text).RootElement.Clone());
    }

    [Theory]
    [MemberData(nameof(EnvironmentIds))]
    public async Task ReturnsATableForEachEnvironment(string environment)
    {
        var (response, table) = await Get($"/land/{environment}?size=large&seed=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Content.Core.Version, response.Headers.GetValues("Imperator-Content-Version").Single());
        var expected = LandTableGenerator.Generate(Content, environment, "large", 3);
        Assert.Equal(expected.Id, table.GetProperty("id").GetString());
        Assert.Equal(48, table.GetProperty("board").GetProperty("width").GetInt32());
        Assert.Equal(16, table.GetProperty("sectors").GetArrayLength());
        Assert.Equal(expected.Cover.Count, table.GetProperty("cover").GetArrayLength());
        Assert.Equal(expected.Conditions, table.GetProperty("conditions").GetString());
        Assert.Equal(Content.Core.Version, table.GetProperty("content_version").GetString());
    }

    [Fact]
    public async Task SizeDefaultsToSmall()
    {
        var (_, table) = await Get("/land/urban?seed=3");
        Assert.Equal(36, table.GetProperty("board").GetProperty("width").GetInt32());
        Assert.Equal("small", table.GetProperty("size").GetString());
    }

    [Theory]
    [InlineData("/land/urban", HttpStatusCode.BadRequest, "seed is required")]
    [InlineData("/land/urban?seed=1&size=huge", HttpStatusCode.BadRequest, "'huge'")]
    [InlineData("/land/moon?seed=1", HttpStatusCode.NotFound, "'moon'")]
    public async Task RejectsBadRequests(string url, HttpStatusCode status, string detail)
    {
        var (response, problem) = await Get(url);
        Assert.Equal(status, response.StatusCode);
        Assert.Contains(detail, problem.GetProperty("detail").GetString());
    }
}
