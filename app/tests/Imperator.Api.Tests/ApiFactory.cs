using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Imperator.Api.Tests;

/// <summary>Runs the API in memory against a database address that refuses connections, so no Postgres is needed.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=2";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Imperator", UnreachableDatabase);
}
