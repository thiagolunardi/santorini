using Santorini.Board;

namespace Santorini.Commands;

public class MoveCommand
{
    public MoveCommand(string playerName, int workerNumber, Coordinate moveTo, Coordinate buildAt)
    {
        ArgumentNullException.ThrowIfNull(moveTo);
        ArgumentNullException.ThrowIfNull(buildAt);

        PlayerName = playerName;
        WorkerNumber = workerNumber;
        MoveTo = moveTo;
        BuildAt = buildAt;
    }

    public Coordinate MoveTo { get; }
    public Coordinate BuildAt { get; }

    public string PlayerName { get; }
    public int WorkerNumber { get; }

    public bool IsValid
    {
        get
        {
            if (string.IsNullOrEmpty(PlayerName))
                return false;

            if (WorkerNumber is < 1 or > 2)
                return false;

            if (MoveTo.Equals(BuildAt))
                return false;

            if (!MoveTo.IsValid)
                return false;

            if (!BuildAt.IsValid)
                return false;

            return true;
        }
    }
}