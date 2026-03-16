namespace Santorini;

public abstract class Piece
{
    protected Piece()
    {
        Id = Guid.NewGuid();
        CurrentLand = null;
    }

    public Guid Id { get; }
    public Land CurrentLand { get; private set; }

    public bool IsPlaced
        => CurrentLand != null;

    internal void SetLand(Land land)
    {
        if (land is null) throw new ArgumentNullException(nameof(land));

        CurrentLand = land;
    }
}