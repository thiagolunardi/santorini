using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Santorini.Tests")]

namespace Santorini;

public class Game
{
    private readonly List<MoveCommand> _movesHistory;

    private readonly List<Player> _players;

    public Game()
    {
        Island = new Island();
        _players = new List<Player>();
        _movesHistory = new List<MoveCommand>();
        Winner = default;
    }

    public Island Island { get; }
    public IReadOnlyCollection<Player> Players => _players;
    public IReadOnlyCollection<MoveCommand> MovesHistory => _movesHistory;

    public Player Winner { get; private set; }

    public IEnumerable<Worker> Workers
    {
        get
        {
            foreach (var land in Island.Board)
                if (land.HasWorker)
                    yield return land.Worker;
        }
    }

    public bool GameIsOver
        => Winner != null;

    public IEnumerable<MoveCommand> GetAvailableMoves(string playerName)
    {
        var player =
            _players.SingleOrDefault(p => p.Name.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));
        if (player == null) yield break;

        foreach (var worker in player.Workers)
        {
            if (worker.CurrentLand == null) continue;

            var currentX = worker.CurrentLand.Coord.X;
            var currentY = worker.CurrentLand.Coord.Y;

            // Try all 8 adjacent cells for move
            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                var moveX = currentX + dx;
                var moveY = currentY + dy;

                if (!Island.IsValidPosition(moveX, moveY)) continue;

                // Try all 8 adjacent cells for build (relative to post-move position)
                for (var bdx = -1; bdx <= 1; bdx++)
                for (var bdy = -1; bdy <= 1; bdy++)
                {
                    if (bdx == 0 && bdy == 0) continue;

                    var buildX = moveX + bdx;
                    var buildY = moveY + bdy;

                    if (!Island.IsValidPosition(buildX, buildY)) continue;

                    // Use canonical player name to ensure case-sensitive matching downstream
                    var command = new MoveCommand(player.Name, worker.Number, new Coord(moveX, moveY),
                        new Coord(buildX, buildY));
                    if (IsMoveCommandAllowed(command)) yield return command;
                }
            }
        }
    }

    public bool TryAddPlayer(string name)
    {
        if (_players.Count >= 2)
            return false;

        if (_players.Any(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            return false;

        _players.Add(new Player(name));

        return true;
    }

    public bool TryAddWorker(string playerName, int workerNumber, int posX, int posY)
    {
        if (Players.Count != 2) return false;

        var player =
            Players.SingleOrDefault(p => p.Name.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));
        if (player is null) return false;

        var worker = player.Workers.SingleOrDefault(b => b.Number == workerNumber);
        if (worker is null || worker.IsPlaced) return false;

        var opponentWorkersOnBoard =
            Workers.Count(b => !b.Player.Name.Equals(playerName, StringComparison.InvariantCultureIgnoreCase));
        if (opponentWorkersOnBoard == 1) return false;

        return Island.TryAddPiece(worker, posX, posY);
    }

    public bool TryMoveWorker(MoveCommand command)
    {
        if (!IsMoveCommandAllowed(command))
            return false;

        var worker = Island.GetWorker(command.PlayerName, command.WorkerNumber);
        if (worker is null) return false;

        var success = worker.TryMoveTo(command.MoveTo.X, command.MoveTo.Y);

        if (success && worker.LandLevel == 3)
        {
            Winner = worker.Player;
            _movesHistory.Add(command);

            return true;
        }

        success = worker.TryBuildAt(command.BuildAt.X, command.BuildAt.Y);

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
        var moveDx = Math.Abs(currentLand.Coord.X - command.MoveTo.X);
        var moveDy = Math.Abs(currentLand.Coord.Y - command.MoveTo.Y);
        if (moveDx > 1 || moveDy > 1) return false;

        // validate if worker can move to destination (unoccupied, not capped, climb at most 1 level)
        if (!Island.TryGetLand(command.MoveTo.X, command.MoveTo.Y, out var moveToLand)) return false;
        if (!moveToLand.IsUnoccupied) return false;
        if (moveToLand.LandLevel - currentLand.LandLevel > 1) return false;

        // validate build destination is adjacent to the POST-MOVE position
        var buildDx = Math.Abs(command.MoveTo.X - command.BuildAt.X);
        var buildDy = Math.Abs(command.MoveTo.Y - command.BuildAt.Y);
        if (buildDx > 1 || buildDy > 1) return false;

        // validate if worker can build after moving (unoccupied or the worker's original cell, not capped)
        if (!Island.TryGetLand(command.BuildAt.X, command.BuildAt.Y, out var buildAtLand)) return false;
        return buildAtLand.IsUnoccupied || buildAtLand == currentLand;
    }
}