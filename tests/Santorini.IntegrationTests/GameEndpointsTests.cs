using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Santorini.IntegrationTests;

public class GameEndpointsTests(GameEndpointsFixture fixture) : IClassFixture<GameEndpointsFixture>
{
    private readonly HttpClient _client = fixture.Client;

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