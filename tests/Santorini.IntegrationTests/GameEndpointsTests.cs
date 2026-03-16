using System.Net;
using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Projects;
using Xunit;

namespace Santorini.IntegrationTests;

public class GameEndpointsTests : IAsyncLifetime
{
    private DistributedApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Santorini_AppHost>();
        _app = await appHost.BuildAsync();
        await _app.StartAsync();
        _client = _app.CreateHttpClient("api");
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task GetState_ReturnsOk()
    {
        var response = await _client.GetAsync("/game/state");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTurn_ReturnsOk()
    {
        var response = await _client.GetAsync("/game/turn");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reset_ReturnsOk()
    {
        var response = await _client.PostAsync("/game/reset", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Move_WithValidMove_ReturnsOk()
    {
        // Player1 starts with worker 1 at (0,0); move to (1,0) and build back at (0,0)
        var moveRequest = new
        {
            PlayerName = "Player1",
            WorkerNumber = 1,
            MoveToX = 1,
            MoveToY = 0,
            BuildAtX = 0,
            BuildAtY = 0
        };

        var response = await _client.PostAsJsonAsync("/game/move", moveRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Mcp_Endpoint_IsReachable()
    {
        var response = await _client.GetAsync("/mcp");

        ((int)response.StatusCode).Should().BeLessThan(500);
    }
}
