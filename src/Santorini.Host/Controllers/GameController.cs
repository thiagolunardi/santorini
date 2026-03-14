using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Santorini;
using Santorini.Host.Services;

namespace Santorini.Host.Controllers
{
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
            var board = new List<object>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var l = game.Island.Board[i, j];
                    board.Add(new
                    {
                        X = i,
                        Y = j,
                        Level = l.LandLevel,
                        HasWorker = l.HasWorker,
                        WorkerOwner = l.Worker?.Player.Name,
                        WorkerNumber = l.Worker?.Number
                    });
                }
            }

            return Ok(new
            {
                Board = board,
                Players = game.Players.Select(p => p.Name).ToArray(),
                Winner = game.Winner?.Name,
                GameOver = game.GameIsOver
            });
        }

        [HttpGet("turn")]
        public IActionResult GetTurn()
        {
            var currentPlayer = _gameService.GetCurrentPlayer();
            if (currentPlayer == null) return NotFound("Game over or no players found.");
            return Ok(new { CurrentPlayer = currentPlayer.Name });
        }

        [HttpPost("move")]
        public IActionResult Move([FromBody] MoveRequest request)
        {
            var result = _gameService.Move(request.PlayerName, request.WorkerNumber, request.MoveToX, request.MoveToY, request.BuildAtX, request.BuildAtY);
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
}
