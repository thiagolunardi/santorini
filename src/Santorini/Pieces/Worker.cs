using Santorini.Board;

namespace Santorini.Pieces;

public class Worker : Piece, IEquatable<Worker>
{
    internal Worker(Player player, int number)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(number, 0);

        Player = player;
        Number = number;
    }

    public Player Player { get; }
    public int Number { get; }

    public int LandLevel
        => CurrentLand?.LandLevel ?? -1;

    public bool Equals(Worker? other)
    {
        if (other is null) return false;

        return Player.Equals(other.Player)
               && Number == other.Number;
    }

    public bool TryMoveTo(Coordinate coordinate)
    {
        if (!CanMoveTo(coordinate, out var land)) return false;
        
        CurrentLand?.TryRemoveWorker(this);
        return land!.TryPutPiece(this);
    }

    public bool TryBuildAt(Coordinate coordinate)
    {
        if (CanBuildAt(coordinate, out var land))
        {
            if (land!.HasTower && !land.Tower!.IsComplete)
            {
                land.Tower.RaiseLevel();

                return true;
            }

            var newBuilding = new Tower();
            return land.TryPutPiece(newBuilding);
        }

        return false;
    }

    private bool CanMoveTo(Coordinate coordinate, out Land? land)
    {
        land = null;

        if (CurrentLand is null) return false;

        if (CurrentLand.Island.TryGetLand(coordinate, out land))
        {
            var from = CurrentLand!;
            var to = land!;

            if (IsLandBlocked(land!)) return false;

            if (IsMovingMoreThan2StepsAway(from, to))
                return false;

            if (IsSteppingUp(from, to) && IsClimbingMoreThen1LevelUp(from, to))
                return false;

            return true;
        }

        return false;
    }

    private bool IsClimbingMoreThen1LevelUp(Land from, Land to)
    {
        var levelDiff = to.LandLevel - from.LandLevel;
        return levelDiff > 1;
    }

    private bool IsSteppingUp(Land from, Land to)
    {
        return to.LandLevel > from.LandLevel;
    }

    private bool IsMovingMoreThan2StepsAway(Land from, Land to)
    {
        var posXdiff = Math.Abs(from.Coordinate.X - to.Coordinate.X);
        var posYdiff = Math.Abs(from.Coordinate.Y - to.Coordinate.Y);

        if (posXdiff > 1 || posYdiff > 1) return true;
        return false;
    }

    private bool IsLandBlocked(Land land)
    {
        return land.Equals(CurrentLand)
               || land.HasWorker
               || land.MaxLevelReached;
    }

    private bool CanBuildAt(Coordinate coordinate, out Land? land)
    {
        land = null;
        if (CurrentLand is null) return false;
        
        if (CurrentLand.Island.TryGetLand(coordinate, out land))
        {
            if (IsLandBlocked(land!)) return false;

            var (posXDiff, posYDiff) = (Math.Abs(CurrentLand.Coordinate.X - land!.Coordinate.X),
                Math.Abs(CurrentLand.Coordinate.Y - land.Coordinate.Y));
            
            if (posXDiff > 1 || posYDiff > 1) return false;

            return true;
        }

        return false;
    }
}