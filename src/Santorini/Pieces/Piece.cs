using Santorini.Board;

namespace Santorini.Pieces;

public abstract class Piece: IEquatable<Piece>
{
    public Guid Id { get; } = Guid.NewGuid();
    public Land? CurrentLand { get; private set; }

    public bool IsPlaced
        => CurrentLand is not null;

    internal void SetLand(Land land)
    {
        ArgumentNullException.ThrowIfNull(land);

        CurrentLand = land;
    }

    public bool Equals(Piece? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Equals(CurrentLand, other.CurrentLand);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Piece)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}