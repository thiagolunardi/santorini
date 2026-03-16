using Santorini.Pieces;

namespace Santorini.Board;

public class Island
{
    public readonly Land[,] Board;

    internal Island()
    {
        Board = new Land[5, 5];

        for (var x = 0; x <= 4; x++)
        for (var y = 0; y <= 4; y++)
            Board[x, y] = new Land(this, new Coordinate(x, y));
    }

    private static (int X, int Y) MaxPositions => (4, 4);

    public bool IsUnoccupied(int posX, int posY)
    {
        return Board[posX, posY].IsUnoccupied;
    }

    public bool TryGetLand(Coordinate coordinate, out Land? land)
    {
        land = null;
        if (!IsValidCoordinate(coordinate)) return false;

        land = Board[coordinate.X, coordinate.Y];

        return true;
    }

    public bool TryAddPiece(Piece piece, Coordinate coordinate)
    {
        return TryGetLand(coordinate, out var land) && 
               land!.TryPutPiece(piece);
    }

    public Worker? GetWorker(string playerName, int workerNumber)
    {
        foreach (var land in Board)
        {
            if (land.HasWorker
                && land.Worker!.Player.Name == playerName
                && land.Worker.Number == workerNumber)
                return land.Worker;
        }

        return null;
    }

    public static bool IsValidCoordinate(int posX, int posY)
    {
        return posX >= 0 &&
               posY >= 0 &&
               posX <= MaxPositions.X &&
               posY <= MaxPositions.Y;
    }

    public static bool IsValidCoordinate(Coordinate coordinate)
    {
        return coordinate is { X: >= 0, Y: >= 0 } && 
               coordinate.X <= MaxPositions.X && 
               coordinate.Y <= MaxPositions.Y;
    }

    public static void AssertCoordinates(int posX, int posY)
    {
        AssertCoordinateX(posX);
        AssertCoordinateY(posY);
    }

    private static void AssertCoordinateX(int posX)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(posX, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(posX, MaxPositions.X);
    }

    private static void AssertCoordinateY(int posY)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(posY, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(posY, MaxPositions.Y);
    }
}