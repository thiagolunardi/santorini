using System.ComponentModel;
using ModelContextProtocol.Server;
using Santorini.Host.Models;
using Santorini.Host.Services;

namespace Santorini.Host.Mcp;

[McpServerToolType]
public class GameMcpTools
{
    private readonly IGameService _gameService;

    public GameMcpTools(IGameService gameService)
    {
        _gameService = gameService;
    }

    [McpServerTool]
    [Description("Gets the current state of the Santorini board, including worker positions and building levels.")]
    public GameStateDto GetState()
    {
        var game = _gameService.GetGame();
        return GameStateDto.FromGame(game);
    }

    [McpServerTool]
    [Description("Gets the current player's turn and all their available legal moves.")]
    public object GetTurn()
    {
        var currentPlayer = _gameService.GetCurrentPlayer();
        if (currentPlayer == null) return new { Message = "Game over or no players found." };

        var availableMoves = _gameService.GetAvailableMoves(currentPlayer.Name)
            .Select(m => new
            {
                m.WorkerNumber,
                MoveTo = new { m.MoveTo.X, m.MoveTo.Y },
                BuildAt = new { m.BuildAt.X, m.BuildAt.Y }
            });

        return new
        {
            CurrentPlayer = currentPlayer.Name,
            AvailableMoves = availableMoves
        };
    }

    [McpServerTool]
    [Description("Moves a worker to a new position and builds a structure on an adjacent cell.")]
    public object MoveWorker(
        [Description("The name of the player (e.g., 'Player1')")]
        string playerName,
        [Description("The worker number (1 or 2)")]
        int workerNumber,
        [Description("X coordinate to move to")]
        int moveToX,
        [Description("Y coordinate to move to")]
        int moveToY,
        [Description("X coordinate to build at")]
        int buildAtX,
        [Description("Y coordinate to build at")]
        int buildAtY)
    {
        var result = _gameService.Move(playerName, workerNumber, moveToX, moveToY, buildAtX, buildAtY);
        if (result)
            return new { Success = true };

        return new { Success = false, Message = "Invalid move. Check available moves for legal options." };
    }

    [McpServerTool]
    [Description("Resets the game to its initial state.")]
    public object ResetGame()
    {
        _gameService.Reset();
        return new { Success = true };
    }
}