using System.Diagnostics;
using Santorini.Pieces;

namespace Santorini.Board;

[DebuggerDisplay(
    "[{Coordinate.X} ,{Coordinate.Y}], Unoccupied: {IsUnoccupied}, Tower: {HasTower}, Worker: {HasWorker}, Level: {LandLevel}")]
public class Land : IEquatable<Land>
{
    private readonly List<Piece> _pieces;
    public readonly Coordinate Coordinate;

    internal Land(Island island, Coordinate coordinate)
    {
        ArgumentNullException.ThrowIfNull(island);
        Island.AssertCoordinates(coordinate.X, coordinate.Y);

        Island = island;
        Coordinate = coordinate;
        _pieces = [];
    }

    public Piece[] Pieces => _pieces.ToArray();

    public Island Island { get; }

    public bool IsUnoccupied
        => !HasWorker && !MaxLevelReached;

    public bool HasTower
        => _pieces.Any(p => p is Tower);

    public Tower? Tower
        => _pieces.SingleOrDefault(p => p is Tower) as Tower;

    public bool HasWorker
        => _pieces.Any(p => p is Worker);

    public Worker? Worker
        => _pieces.SingleOrDefault(p => p is Worker) as Worker;

    public int LandLevel
        => HasTower
            ? Tower!.Level
            : 0;

    public bool MaxLevelReached
        => HasTower && Tower!.IsComplete;

    public bool Equals(Land? other)
    {
        if (other is null) return false;
        return Coordinate.X == other.Coordinate.X && Coordinate.Y == other.Coordinate.Y;
    }

    public bool TryPutPiece(Piece piece)
    {
        if (piece is Tower && (HasTower || HasWorker))
            return false;

        if (piece is Worker worker)
        {
            if (HasWorker) return false;

            if (worker.CurrentLand is not null && LandLevel > worker.LandLevel + 1)
                return false;
        }

        _pieces.Add(piece);
        piece.SetLand(this);

        return true;
    }

    public bool TryRemoveWorker(Worker worker)
    {
        if (!HasWorker) return false;

        if (!Worker!.Equals(worker)) return false;

        _pieces.Remove(Worker);

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return (obj as Land)!.Equals(this);
    }

    public static bool operator ==(Land? land1, Land? land2)
    {
        if (land1 is null && land2 is null) return true;
        if (land1 is null || land2 is null) return false;
        return land1.Coordinate == land2.Coordinate;
    }

    public static bool operator !=(Land land1, Land land2)
    {
        return !(land1 == land2);
    }

    public override int GetHashCode()
    {
        return Coordinate.GetHashCode();
    }
}