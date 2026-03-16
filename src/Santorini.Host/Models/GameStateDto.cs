using System.Collections.Generic;
using System.Linq;
using Santorini;

namespace Santorini.Host.Models
{
    public sealed class BoardCellDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Level { get; set; }
        public bool HasWorker { get; set; }
        public string? WorkerOwner { get; set; }
        public int? WorkerNumber { get; set; }
    }

    public sealed class GameStateDto
    {
        public List<BoardCellDto> Board { get; set; } = new List<BoardCellDto>();
        public string[] Players { get; set; } = new string[0];
        public string? Winner { get; set; }
        public bool GameOver { get; set; }

        public static GameStateDto FromGame(Game game)
        {
            var board = new List<BoardCellDto>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var l = game.Island.Board[i, j];
                    board.Add(new BoardCellDto
                    {
                        X = i,
                        Y = j,
                        Level = l.LandLevel,
                        HasWorker = l.HasWorker,
                        WorkerOwner = l.Worker?.Player.Name,
                        WorkerNumber = l.Worker?.Number
                    });
                }
            }

            return new GameStateDto
            {
                Board = board,
                Players = game.Players.Select(p => p.Name).ToArray(),
                Winner = game.Winner?.Name,
                GameOver = game.GameIsOver
            };
        }
    }
}
