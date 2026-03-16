using Santorini.Board;
using Santorini.Commands;
using Santorini.Pieces;

namespace Santorini;

public class Game
{
    private readonly List<MoveCommand> _movesHistory = [];
    private readonly List<Player> _players = [];
    private IEnumerable<Worker> Workers
    {
        get
        {
            foreach (var land in Island.Board)
                if (land.HasWorker)
                    yield return land.Worker!;
        }
    }

    public Island Island { get; } = new();
    public IReadOnlyCollection<Player> Players => _players;
    public IReadOnlyCollection<MoveCommand> MovesHistory => _movesHistory;
    public Player? Winner { get; private set; }


    public bool GameIsOver
        => Winner != null;

    public IEnumerable<MoveCommand> GetAvailableMoves(string playerName)
    {
        var player =
            _players.SingleOrDefault(p => p.Name.Equals(playerName));
        if (player == null) yield break;

        foreach (var worker in player.Workers)
        {
            if (worker.CurrentLand == null) continue;

            var currentCoordinate = worker.CurrentLand.Coordinate;

            // Try all 8 adjacent cells for move
            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                var moveToX = currentCoordinate.X + dx;
                var moveToY = currentCoordinate.Y + dy;
                if (!Island.IsValidCoordinate(moveToX, moveToY)) continue;

                var moveTo = new Coordinate(currentCoordinate.X + dx, currentCoordinate.Y + dy);

                // Try all 8 adjacent cells for build (relative to post-move position)
                for (var adjacentX = -1; adjacentX <= 1; adjacentX++)
                for (var adjacentY = -1; adjacentY <= 1; adjacentY++)
                {
                    if (adjacentX == 0 && adjacentY == 0) continue;

                    var buildAtX = currentCoordinate.X + adjacentX;
                    var buildAtY = currentCoordinate.Y + adjacentY;
                    if (!Island.IsValidCoordinate(buildAtX, buildAtY)) continue;
                    
                    var buildAt = new Coordinate(currentCoordinate.X + adjacentX, currentCoordinate.Y + adjacentY);

                    // Use canonical player name to ensure case-sensitive matching downstream
                    var command = new MoveCommand(player.Name, worker.Number, moveTo, buildAt);
                    if (IsMoveCommandAllowed(command)) yield return command;
                }
            }
        }
    }

    public bool TryAddPlayer(string name)
    {
        if (_players.Count >= 2)
            return false;

        if (_players.Any(p => p.Name.Equals(name)))
            return false;

        _players.Add(new Player(name));

        return true;
    }

    public bool TryAddWorker(string playerName, int workerNumber, Coordinate coordinate)
    {
        if (Players.Count != 2) return false;

        var player =
            Players.SingleOrDefault(p => p.Name.Equals(playerName));

        var worker = player?.Workers.SingleOrDefault(b => b.Number == workerNumber);
        if (worker is null || worker.IsPlaced) return false;

        var opponentWorkersOnBoard =
            Workers.Count(b => !b.Player.Name.Equals(playerName));
        if (opponentWorkersOnBoard == 1) return false;

        return Island.TryAddPiece(worker, coordinate);
    }

    public bool TryMoveWorker(MoveCommand command)
    {
        if (!IsMoveCommandAllowed(command))
            return false;

        var worker = Island.GetWorker(command.PlayerName, command.WorkerNumber);
        if (worker is null) return false;

        var success = worker.TryMoveTo(command.MoveTo);

        if (success && worker.LandLevel == 3)
        {
            Winner = worker.Player;
            _movesHistory.Add(command);

            return true;
        }

        success = worker.TryBuildAt(command.BuildAt);

        if (success)
            _movesHistory.Add(command);

        return success;
    }

    private bool IsMoveCommandAllowed(MoveCommand command)
    {
        // is command a valid one
        if (!command.IsValid) return false;

        // validate the worker owns the player
        var player = Players.SingleOrDefault(p => p.Name == command.PlayerName);
        if (player is null) return false;

        var worker = player.Workers.SingleOrDefault(b => b.Number == command.WorkerNumber);
        if (worker is null || worker.CurrentLand is null) return false;

        var currentLand = worker.CurrentLand;

        // validate move destination is adjacent (within 1 step in each direction)
        var moveDx = Math.Abs(currentLand.Coordinate.X - command.MoveTo.X);
        var moveDy = Math.Abs(currentLand.Coordinate.Y - command.MoveTo.Y);
        if (moveDx > 1 || moveDy > 1) return false;

        // validate if worker can move to destination (unoccupied, not capped, climb at most 1 level)
        if (!Island.TryGetLand(command.MoveTo, out var moveToLand)) return false;
        if (!moveToLand!.IsUnoccupied) return false;
        if (moveToLand.LandLevel - currentLand.LandLevel > 1) return false;

        // validate build destination is adjacent to the POST-MOVE position
        var buildDx = Math.Abs(command.MoveTo.X - command.BuildAt.X);
        var buildDy = Math.Abs(command.MoveTo.Y - command.BuildAt.Y);
        if (buildDx > 1 || buildDy > 1) return false;

        // validate if worker can build after moving (unoccupied or the worker's original cell, not capped)
        if (!Island.TryGetLand(command.BuildAt, out var buildAtLand)) return false;
        return buildAtLand!.IsUnoccupied || buildAtLand == currentLand;
    }
}