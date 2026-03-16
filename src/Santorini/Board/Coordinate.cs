namespace Santorini.Board;

public class Coordinate : IEquatable<Coordinate>
{
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        Island.AssertCoordinates(x, y);

        X = x;
        Y = y;
    }

    public bool IsValid
        => Island.IsValidCoordinate(this);

    public bool Equals(Coordinate? obj)
    {
        if (obj is null) return false;
        return X == obj.X && Y == obj.Y;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return (obj as Coordinate)!.Equals(this);
    }

    public static bool operator ==(Coordinate? coordinate1, Coordinate? coordinate2)
    {
        if (coordinate1 is null && coordinate2 is null) return true;
        if (coordinate1 is null || coordinate2 is null) return false;
        return coordinate1.X == coordinate2.X && coordinate1.Y == coordinate2.Y;
    }

    public static bool operator !=(Coordinate coordinate1, Coordinate coordinate2)
    {
        return !(coordinate1 == coordinate2);
    }

    public override int GetHashCode()
    {
        var hashCode = -665186484;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        return hashCode;
    }
}