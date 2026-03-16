using Microsoft.AspNetCore.Mvc;
using Santorini.Host.Models;
using Santorini.Host.Services;

namespace Santorini.Host.Controllers;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        var game = _gameService.GetGame();
        return Ok(GameStateDto.FromGame(game));
    }

    [HttpGet("turn")]
    public IActionResult GetTurn()
    {
        var currentPlayer = _gameService.GetCurrentPlayer();
        if (currentPlayer == null) return NotFound("Game over or no players found.");

        var availableMoves = _gameService.GetAvailableMoves(currentPlayer.Name)
            .Select(m => new
            {
                m.WorkerNumber,
                MoveTo = new { m.MoveTo.X, m.MoveTo.Y },
                BuildAt = new { m.BuildAt.X, m.BuildAt.Y }
            });

        return Ok(new
        {
            CurrentPlayer = currentPlayer.Name,
            AvailableMoves = availableMoves
        });
    }

    [HttpPost("move")]
    public IActionResult Move([FromBody] MoveRequest request)
    {
        var result = _gameService.Move(request.PlayerName, request.WorkerNumber, request.MoveToX, request.MoveToY,
            request.BuildAtX, request.BuildAtY);
        if (result)
            return Ok(new { Success = true });

        return BadRequest(new { Success = false, Message = "Invalid move" });
    }

    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _gameService.Reset();
        return Ok(new { Success = true });
    }
}

public class MoveRequest
{
    public string PlayerName { get; set; } = string.Empty;
    public int WorkerNumber { get; set; }
    public int MoveToX { get; set; }
    public int MoveToY { get; set; }
    public int BuildAtX { get; set; }
    public int BuildAtY { get; set; }
}