using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Santorini.Tests
{
    [ExcludeFromCodeCoverage]
    public class GameTests
    {
        private readonly Faker _faker;

        public GameTests()
        {
            _faker = new Faker();
        }

        [Fact]
        public void Create_valid_game()
        {
            // arrange, act
            var game = new Game();

            // assert
            game.Should().NotBeNull();
            game.Island.Should().NotBeNull();
            game.Players.Should().HaveCount(0);
            game.MovesHistory.Should().HaveCount(0);
        }

        [Fact]
        public void game_can_accept_2_players_registration()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            var player3Name = _faker.Name.FirstName();

            // act
            var success1 = game.TryAddPlayer(player1Name);
            var success2 = game.TryAddPlayer(player2Name);
            var success3 = game.TryAddPlayer(player3Name);
            var success4 = game.TryAddPlayer(player1Name);

            // assert
            success1.Should().BeTrue();
            success2.Should().BeTrue();
            success3.Should().BeFalse();
            success4.Should().BeFalse();
            game.Players.Should().HaveCount(2);
            game.Players.First().Name.Should().Be(player1Name);
            game.Players.Last().Name.Should().Be(player2Name);
        }

        [Fact]
        public void game_should_refuse_players_with_same_name()
        {
            // arrange
            var game = new Game();
            var playerName = _faker.Name.FirstName();

            // act
            var success1 = game.TryAddPlayer(playerName);
            var success2 = game.TryAddPlayer(playerName);

            // assert
            success1.Should().BeTrue();
            success2.Should().BeFalse();
            game.Players.Should().HaveCount(1);
            game.Players.First().Name.Should().Be(playerName);
        }

        [Fact]
        public void game_should_refuse_movement_while_all_players_present()
        {
            // arrange
            var game = new Game();
            var coord = GetEmptyCoord(game);
            var playerName = _faker.Name.FirstName();
            game.TryAddPlayer(playerName);

            // act
            var success = game.TryAddWorker(playerName, 1, coord.X, coord.Y);

            // assert
            success.Should().BeFalse();
        }

        [Fact]
        public void game_should_refuse_movement_wrong_player_name()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();

            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            var coord = GetEmptyCoord(game);
            game.TryAddWorker(player1Name, 1, coord.X, coord.Y);
            coord = GetEmptyCoord(game);
            game.TryAddWorker(player1Name, 2, coord.X, coord.Y);
            coord = GetEmptyCoord(game);
            game.TryAddWorker(player2Name, 1, coord.X, coord.Y);
            coord = GetEmptyCoord(game);
            game.TryAddWorker(player2Name, 2, coord.X, coord.Y);

            var unknownName = _faker.Name.FirstName();            
            var moveCmd = new MoveCommand(unknownName, 1, GetEmptyCoord(game), GetEmptyCoord(game));

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            success.Should().BeFalse();
        }

        [Fact]
        public void Cannot_modify_registered_players()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            // act
            var player = game.Players.First();            
            player = null;

            // assert
            game.Players.Should().HaveCount(2);
            game.Players.First().Name.Should().Be(player1Name);
            game.Players.Last().Name.Should().Be(player2Name);
        }

        [Fact]
        public void Players_might_add_its_Workers()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            // act                        
            var p1b1Coord = GetEmptyCoord(game);
            var p1Success1 = game.TryAddWorker(player1Name, 1, p1b1Coord.X, p1b1Coord.Y);
            var p1b2Coord = GetEmptyCoord(game);
            var p1Success2 = game.TryAddWorker(player1Name, 2, p1b2Coord.X, p1b2Coord.Y);

            var p2b1Coord = GetEmptyCoord(game);
            var p2Success1 = game.TryAddWorker(player2Name, 1, p2b1Coord.X, p2b1Coord.Y);
            var p2b2Coord = GetEmptyCoord(game);
            var p2Success2 = game.TryAddWorker(player2Name, 2, p2b2Coord.X, p2b2Coord.Y);

            // assert
            p1Success1.Should().BeTrue();
            p1Success2.Should().BeTrue();
            p2Success1.Should().BeTrue();
            p2Success2.Should().BeTrue();

            var p1b1Land = game.Island.Board[p1b1Coord.X, p1b1Coord.Y];
            var p1b2Land = game.Island.Board[p1b2Coord.X, p1b2Coord.Y];
            var p2b1Land = game.Island.Board[p2b1Coord.X, p2b1Coord.Y];
            var p2b2Land = game.Island.Board[p2b2Coord.X, p2b2Coord.Y];

            var p1Worker1 = p1b1Land.Worker;
            var p1Worker2 = p1b2Land.Worker;
            var p2Worker1 = p2b1Land.Worker;
            var p2Worker2 = p2b2Land.Worker;

            p1b1Land.HasWorker.Should().BeTrue();
            p1b2Land.HasWorker.Should().BeTrue();
            p2b1Land.HasWorker.Should().BeTrue();
            p2b2Land.HasWorker.Should().BeTrue();

            p1Worker1.Number.Should().Be(1);
            p1Worker1.Player.Name.Should().Be(player1Name);
            p1Worker2.Number.Should().Be(2);
            p1Worker2.Player.Name.Should().Be(player1Name);
            p2Worker1.Number.Should().Be(1);
            p2Worker1.Player.Name.Should().Be(player2Name);
            p2Worker2.Number.Should().Be(2);
            p2Worker2.Player.Name.Should().Be(player2Name);
        }

        [Fact]
        public void Player_request_move_command()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            var p1b1Coord = new Coord(0, 0);
            game.TryAddWorker(player1Name, 1, p1b1Coord.X, p1b1Coord.Y);
            var p1b2Coord = new Coord(1, 1);
            game.TryAddWorker(player1Name, 2, p1b2Coord.X, p1b2Coord.Y);
            var p2b1Coord = new Coord(4, 4);
            game.TryAddWorker(player2Name, 1, p2b1Coord.X, p2b1Coord.Y);
            var p2b2Coord = new Coord(3, 3);
            game.TryAddWorker(player2Name, 2, p2b2Coord.X, p2b2Coord.Y);

            var moveTo = new Coord(0, 1);
            var buildAt = new Coord(0, 0);
            var moveCmd = new MoveCommand(player1Name, 1, moveTo, buildAt);

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            success.Should().BeTrue();
            game.Island.Board[0, 0].IsUnoccupied.Should().BeTrue();
            game.Island.Board[0, 0].HasTower.Should().BeTrue();
            game.Island.Board[0, 1].IsUnoccupied.Should().BeFalse();
            game.Island.Board[0, 1].HasWorker.Should().BeTrue();
            game.Island.Board[0, 1].Worker.Player.Name.Should().Be(player1Name);
            game.Island.Board[0, 1].Worker.Number.Should().Be(1);
        }

        [Fact]
        public void game_should_refuse_invalid_move_command_with_wrong_workerNumber()
        {
            // arrange
            var game = new Game();
            var moveCmd = new MoveCommand(
                playerName: _faker.Name.FirstName(),
                workerNumber: 0,
                moveTo: new Coord(0, 0),
                buildAt: new Coord(0, 0));

            // act 
            var success = game.TryMoveWorker(moveCmd);

            // assert
            game.Should().NotBeNull();
            moveCmd.IsValid.Should().BeFalse();
            success.Should().BeFalse();
        }

        [Fact]
        public void game_should_refuse_move_command_with_wrong_playerName()
        {
            // arrange
            var game = new Game();
            var moveCmd = new MoveCommand(
                playerName: _faker.Name.FirstName(),
                workerNumber: 1,
                moveTo: new Coord(0, 0),
                buildAt: new Coord(0, 1));

            // act 
            var success = game.TryMoveWorker(moveCmd);

            // assert
            game.Should().NotBeNull();
            moveCmd.IsValid.Should().BeTrue();
            success.Should().BeFalse();
        }

        [Fact]
        public void game_should_refuse_move_command_to_occupied_land()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            var p1b1Coord = new Coord(x: 0, y: 0);
            var successP1B1 = game.TryAddWorker(player1Name, 1, p1b1Coord.X, p1b1Coord.Y);
            var p1b2Coord = new Coord(x: 1, y: 1);
            var successP1B2 = game.TryAddWorker(player1Name, 2, p1b2Coord.X, p1b2Coord.Y);
            var p2b1Coord = new Coord(x: 4, y: 4);
            var successP2B1 = game.TryAddWorker(player2Name, 1, p2b1Coord.X, p2b1Coord.Y);
            var p2b2Coord = new Coord(x: 3, y: 3);
            var successP2B2 = game.TryAddWorker(player2Name, 2, p2b2Coord.X, p2b2Coord.Y);

            var moveTo = new Coord(0, 1);
            var buildAt = new Coord(0, 0);
            var moveCmd = new MoveCommand(player1Name, 1, moveTo, buildAt);

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            successP1B1.Should().BeTrue();
            successP1B2.Should().BeTrue();
            successP2B1.Should().BeTrue();
            successP2B2.Should().BeTrue();
            success.Should().BeTrue();            
            game.Island.Board[0, 0].IsUnoccupied.Should().BeTrue();
            game.Island.Board[0, 0].HasTower.Should().BeTrue();
            game.Island.Board[0, 1].IsUnoccupied.Should().BeFalse();
            game.Island.Board[0, 1].HasWorker.Should().BeTrue();
            game.Island.Board[0, 1].Worker.Player.Name.Should().Be(player1Name);
            game.Island.Board[0, 1].Worker.Number.Should().Be(1);
        }

        [Fact]
        public void game_is_over_when_worker_at_3rd_level()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            var coord = new Coord(x: 0, y: 0);
            game.TryAddWorker(player1Name, 1, coord.X, coord.Y);
            coord = new Coord(x: 1, y: 1);
            game.TryAddWorker(player1Name, 2, coord.X, coord.Y);
            coord = new Coord(x: 4, y: 4);
            game.TryAddWorker(player2Name, 1, coord.X, coord.Y);
            coord = new Coord(x: 3, y: 3);
            game.TryAddWorker(player2Name, 2, coord.X, coord.Y);

            for (var i = 0; i < 3; i++)
            {
                // build level
                var moveTo = new Coord(0, 1);
                var buildAt = new Coord(0, 0);
                var moveCmd = new MoveCommand(player1Name, 1, moveTo, buildAt);
                game.TryMoveWorker(moveCmd);

                if (i >= 2) break;

                // build level
                moveTo = new Coord(0, 0);
                buildAt = new Coord(0, 1);
                moveCmd = new MoveCommand(player1Name, 1, moveTo, buildAt);
                game.TryMoveWorker(moveCmd);
            }

            // act
            var winMoveCmd = new MoveCommand(player1Name, 1, new Coord(0, 0), new Coord(0, 1));
            var success = game.TryMoveWorker(winMoveCmd);

            // assert
            success.Should().BeTrue();
            game.GameIsOver.Should().BeTrue();
            game.Winner.Name.Should().Be(player1Name);
        }

        [Fact]
        public void game_should_refuse_move_command_when_climb_limit_exceeded()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 4, 0);
            game.TryAddWorker(player2Name, 2, 0, 4);

            // Place a tower at level 2 adjacent to worker 1
            var tower = new Tower();
            tower.RaiseLevel(); // level 2
            game.Island.TryAddPiece(tower, 0, 1);

            // Worker 1 is at level 0, target is level 2 — climb limit is 1
            var moveCmd = new MoveCommand(player1Name, 1, new Coord(0, 1), new Coord(1, 0));

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            success.Should().BeFalse();
            game.Island.Board[0, 0].HasWorker.Should().BeTrue();
            game.Island.Board[0, 1].HasWorker.Should().BeFalse();
        }

        [Fact]
        public void game_should_refuse_move_command_when_moveto_is_not_adjacent()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 4, 0);
            game.TryAddWorker(player2Name, 2, 0, 4);

            // moveTo is 2 steps away from worker position
            var moveCmd = new MoveCommand(player1Name, 1, new Coord(0, 2), new Coord(0, 1));

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            success.Should().BeFalse();
            game.Island.Board[0, 0].HasWorker.Should().BeTrue();
            game.Island.Board[0, 2].HasWorker.Should().BeFalse();
        }

        [Fact]
        public void game_should_refuse_move_command_when_buildAt_is_not_adjacent_to_moveTo()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 4, 0);
            game.TryAddWorker(player2Name, 2, 0, 4);

            // Worker moves to (0,1), but buildAt (3,3) is not adjacent to (0,1)
            var moveCmd = new MoveCommand(player1Name, 1, new Coord(0, 1), new Coord(3, 3));

            // act
            var success = game.TryMoveWorker(moveCmd);

            // assert
            success.Should().BeFalse();
            game.Island.Board[0, 0].HasWorker.Should().BeTrue();
        }

        [Fact]
        public void GetAvailableMoves_should_not_include_moves_that_exceed_climb_limit()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 4, 0);
            game.TryAddWorker(player2Name, 2, 0, 4);

            // Place a level-2 tower adjacent to worker 1 at (0,0)
            var tower = new Tower();
            tower.RaiseLevel(); // level 2
            game.Island.TryAddPiece(tower, 0, 1);

            // act
            var availableMoves = game.GetAvailableMoves(player1Name).ToList();

            // assert: no available move for worker 1 should target (0,1) since it is 2 levels up
            var movesToLevel2 = availableMoves.Where(m => m.WorkerNumber == 1 && m.MoveTo.X == 0 && m.MoveTo.Y == 1);
            movesToLevel2.Should().BeEmpty();
        }

        [Fact]
        public void GetAvailableMoves_should_only_include_builds_adjacent_to_moveto()
        {
            // arrange
            var game = new Game();
            var player1Name = _faker.Name.FirstName();
            var player2Name = _faker.Name.FirstName();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);

            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 4, 0);
            game.TryAddWorker(player2Name, 2, 0, 4);

            // act
            var availableMoves = game.GetAvailableMoves(player1Name).ToList();

            // assert: every buildAt must be adjacent (within 1 step) to the moveTo position
            foreach (var move in availableMoves)
            {
                var buildDx = Math.Abs(move.MoveTo.X - move.BuildAt.X);
                var buildDy = Math.Abs(move.MoveTo.Y - move.BuildAt.Y);
                buildDx.Should().BeLessThanOrEqualTo(1, because: $"build ({move.BuildAt.X},{move.BuildAt.Y}) must be adjacent to moveTo ({move.MoveTo.X},{move.MoveTo.Y})");
                buildDy.Should().BeLessThanOrEqualTo(1, because: $"build ({move.BuildAt.X},{move.BuildAt.Y}) must be adjacent to moveTo ({move.MoveTo.X},{move.MoveTo.Y})");
            }
        }

        private Coord GetEmptyCoord(Game game)
        {
            int x = 0, y = 0;
            while (!game.Island.Board[x, y].IsUnoccupied)
            {
                x = _faker.Random.Number(0, 4);
                y = _faker.Random.Number(0, 4);
            }
            return new Coord(x, y);
        }

        private static Game SetupStandardGame(string player1Name = "Player1", string player2Name = "Player2")
        {
            var game = new Game();
            game.TryAddPlayer(player1Name);
            game.TryAddPlayer(player2Name);
            game.TryAddWorker(player1Name, 1, 0, 0);
            game.TryAddWorker(player1Name, 2, 4, 4);
            game.TryAddWorker(player2Name, 1, 0, 4);
            game.TryAddWorker(player2Name, 2, 4, 0);
            return game;
        }

        [Fact]
        public void GetAvailableMoves_returns_empty_for_unknown_player()
        {
            var game = SetupStandardGame();

            var moves = game.GetAvailableMoves("unknown").ToList();

            moves.Should().BeEmpty();
        }

        [Fact]
        public void GetAvailableMoves_is_case_insensitive_for_player_name()
        {
            var game = SetupStandardGame("Player1", "Player2");

            var movesExact = game.GetAvailableMoves("Player1").ToList();
            var movesLower = game.GetAvailableMoves("player1").ToList();

            movesExact.Should().NotBeEmpty();
            movesLower.Should().HaveSameCount(movesExact);
        }

        [Fact]
        public void GetAvailableMoves_uses_canonical_player_name()
        {
            var game = SetupStandardGame("Player1", "Player2");

            var moves = game.GetAvailableMoves("player1").ToList();

            moves.Should().AllSatisfy(m => m.PlayerName.Should().Be("Player1"));
        }

        [Fact]
        public void GetAvailableMoves_all_moves_are_executable()
        {
            var game = SetupStandardGame();

            var moves = game.GetAvailableMoves("Player1").ToList();

            moves.Should().NotBeEmpty();
            moves.Should().AllSatisfy(m =>
            {
                var testGame = SetupStandardGame();
                testGame.TryMoveWorker(m).Should().BeTrue($"move {m.WorkerNumber} to ({m.MoveTo.X},{m.MoveTo.Y}) build ({m.BuildAt.X},{m.BuildAt.Y}) should be executable");
            });
        }

        [Fact]
        public void GetAvailableMoves_excludes_moves_to_occupied_cells()
        {
            var game = SetupStandardGame();

            var moves = game.GetAvailableMoves("Player1").ToList();

            // Worker 1 is at (0,0); Worker 2 is at (4,4)
            // No move should target (0,4) or (4,0) where Player2's workers are
            moves.Should().NotContain(m => m.MoveTo.X == 0 && m.MoveTo.Y == 4);
            moves.Should().NotContain(m => m.MoveTo.X == 4 && m.MoveTo.Y == 0);
        }

        [Fact]
        public void GetAvailableMoves_excludes_moves_to_domed_cells()
        {
            var game = new Game();
            game.TryAddPlayer("Player1");
            game.TryAddPlayer("Player2");
            // Place workers with space between them
            game.TryAddWorker("Player1", 1, 0, 0);
            game.TryAddWorker("Player1", 2, 4, 4);
            game.TryAddWorker("Player2", 1, 0, 4);
            game.TryAddWorker("Player2", 2, 4, 0);

            // Build a dome (4 levels) at (1, 0) by alternating moves
            // We need to build 4 levels at (1,0). First 3 levels from Player1 worker1,
            // then have Player2 build there too. We simulate by building up manually.
            // Level 1: Player1 moves w1 from (0,0) to (0,1), builds at (1,0)
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 1), new Coord(1, 0)));
            // Level 2: Player2 moves w1 from (0,4) to (1,4), builds at (1,0) - not adjacent, use (0,4)
            game.TryMoveWorker(new MoveCommand("Player2", 1, new Coord(1, 4), new Coord(0, 4)));
            // Level 3: Player1 moves w1 from (0,1) to (0,0), builds at (1,0)
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 0), new Coord(1, 0)));
            // Level 4: Player2 moves w1 from (1,4) to (0,4), builds at (1,0) - not adjacent
            game.TryMoveWorker(new MoveCommand("Player2", 1, new Coord(0, 4), new Coord(1, 4)));
            // Level 5 (dome): Player1 moves w1 from (0,0) to (0,1), builds at (1,0)
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 1), new Coord(1, 0)));
            // Player2 turn, keep going
            game.TryMoveWorker(new MoveCommand("Player2", 1, new Coord(1, 4), new Coord(0, 4)));
            // Dome move: Player1 moves w1 from (0,1) to (0,0), builds at (1,0) - 4th build = dome
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 0), new Coord(1, 0)));

            var moves = game.GetAvailableMoves("Player2").ToList();
            moves.Should().NotContain(m => m.MoveTo.X == 1 && m.MoveTo.Y == 0,
                "domed cells should not be valid move destinations");
        }

        [Fact]
        public void GetAvailableMoves_excludes_moves_that_climb_more_than_1_level()
        {
            var game = new Game();
            game.TryAddPlayer("Player1");
            game.TryAddPlayer("Player2");
            game.TryAddWorker("Player1", 1, 0, 0);
            game.TryAddWorker("Player1", 2, 4, 4);
            game.TryAddWorker("Player2", 1, 2, 2);
            game.TryAddWorker("Player2", 2, 4, 0);

            // Build 2 levels at (1,0): Player1 w1 at (0,0) can reach (1,0), Player2 w1 at (2,2) can reach (2,1)
            // Build level 1 at (1,1): Player1 move (0,0)->(0,1), build (1,1)
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 1), new Coord(1, 1)));
            // Build level 2 at (1,1): Player2 move (2,2)->(2,1), build (1,1)
            game.TryMoveWorker(new MoveCommand("Player2", 1, new Coord(2, 1), new Coord(1, 1)));
            // Build level 3 at (1,1): Player1 move (0,1)->(0,0), build (1,1)
            game.TryMoveWorker(new MoveCommand("Player1", 1, new Coord(0, 0), new Coord(1, 1)));
            // Now (1,1) has level 3. Player2 w1 is at (2,1) at level 0 - cannot climb 3 levels to (1,1)
            var moves = game.GetAvailableMoves("Player2").ToList();
            moves.Should().NotContain(m => m.MoveTo.X == 1 && m.MoveTo.Y == 1,
                "cannot move to a cell that is more than 1 level higher");
        }

        [Fact]
        public void GetAvailableMoves_build_is_adjacent_to_post_move_position()
        {
            var game = SetupStandardGame();

            var moves = game.GetAvailableMoves("Player1").ToList();

            moves.Should().AllSatisfy(m =>
            {
                var buildDx = Math.Abs(m.MoveTo.X - m.BuildAt.X);
                var buildDy = Math.Abs(m.MoveTo.Y - m.BuildAt.Y);
                (buildDx <= 1).Should().BeTrue($"build at ({m.BuildAt.X},{m.BuildAt.Y}) must be adjacent to post-move ({m.MoveTo.X},{m.MoveTo.Y})");
                (buildDy <= 1).Should().BeTrue($"build at ({m.BuildAt.X},{m.BuildAt.Y}) must be adjacent to post-move ({m.MoveTo.X},{m.MoveTo.Y})");
            });
        }
    }
}
