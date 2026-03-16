using System.Collections.Generic;
using System.Linq;
using Santorini;

namespace Santorini.Host.Services
{
    public interface IGameService
    {
        Game GetGame();
        Player? GetCurrentPlayer();
        bool Move(string playerName, int workerNumber, int moveX, int moveY, int buildX, int buildY);
        void Reset();
        IEnumerable<MoveCommand> GetAvailableMoves(string playerName);
    }

    public class GameService : IGameService
    {
        private Game _game = null!;
        private readonly object _lock = new object();

        public GameService()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            _game = new Game();
            _game.TryAddPlayer("Player1");
            _game.TryAddPlayer("Player2");

            // Initial worker placement
            _game.TryAddWorker("Player1", 1, 0, 0);
            _game.TryAddWorker("Player1", 2, 4, 4);
            _game.TryAddWorker("Player2", 1, 0, 4);
            _game.TryAddWorker("Player2", 2, 4, 0);
        }

        public Game GetGame() => _game;

        public Player? GetCurrentPlayer()
        {
            if (_game.GameIsOver) return null;
            var players = _game.Players.ToList();
            if (players.Count == 0) return null;
            return players[_game.MovesHistory.Count % 2];
        }

        public bool Move(string playerName, int workerNumber, int moveX, int moveY, int buildX, int buildY)
        {
            lock (_lock)
            {
                var currentPlayer = GetCurrentPlayer();
                if (currentPlayer == null || !currentPlayer.Name.Equals(playerName, System.StringComparison.InvariantCultureIgnoreCase))
                    return false;

                var command = new MoveCommand(currentPlayer.Name, workerNumber, new Coord(moveX, moveY), new Coord(buildX, buildY));
                return _game.TryMoveWorker(command);
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                InitializeGame();
            }
        }

        public IEnumerable<MoveCommand> GetAvailableMoves(string playerName)
        {
            return _game.GetAvailableMoves(playerName);
        }
    }
}
