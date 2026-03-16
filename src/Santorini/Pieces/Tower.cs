namespace Santorini;

public class Tower : Piece
{
    internal Tower()
    {
        Level = 1;
    }

    public int Level { get; private set; }

    public bool IsComplete
        => Level >= 4;

    public int RaiseLevel()
    {
        if (!IsComplete) Level++;
        return Level;
    }
}