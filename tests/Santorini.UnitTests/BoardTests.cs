using System.Diagnostics.CodeAnalysis;
using Bogus;
using FluentAssertions;
using Santorini.Board;
using Santorini.Pieces;
using Xunit;

namespace Santorini.UnitTests;

[ExcludeFromCodeCoverage]
public class BoardTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Board_must_start_with_25_empty_lands()
    {
        var board = new Island();

        board.Board.Should().NotBeNull();
        board.Board.Length.Should().Be(25);
        foreach (var land in board.Board)
        {
            land.Should().NotBeNull();
            land.IsUnoccupied.Should().BeTrue();
            board.IsUnoccupied(land.Coordinate.X, land.Coordinate.Y).Should().BeTrue();
        }

        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
            board.Board[x, y].Should().Be(new Land(board, new (x, y)));
    }

    [Fact]
    public void Board_accept_upto_4_workers()
    {
        // arrange
        var board = new Island();
        var player1 = new Player(_faker.Name.FirstName());
        var player2 = new Player(_faker.Name.FirstName());

        // act
        var success = true;
        success = success && board.TryAddPiece(player1.Workers.First(), new(0, 0));
        success = success && board.TryAddPiece(player1.Workers.Last(), new(0, 1));
        success = success && board.TryAddPiece(player2.Workers.First(), new(0, 2));
        success = success && board.TryAddPiece(player2.Workers.Last(), new(0, 3));

        // assert
        success.Should().BeTrue();
        board.IsUnoccupied(0, 0).Should().BeFalse();
        board.IsUnoccupied(0, 1).Should().BeFalse();
        board.IsUnoccupied(0, 2).Should().BeFalse();
        board.IsUnoccupied(0, 3).Should().BeFalse();
    }

    [Fact]
    public void Board_can_retrieve_worker_by_playernamer_and_workernumber()
    {
        // arrange
        var board = new Island();
        var playerName = _faker.Name.FirstName();
        var player = new Player(playerName);
        var worker1 = player.Workers.First();

        var posX = _faker.Random.Number(0, 4);
        var posY = _faker.Random.Number(0, 4);
        board.TryAddPiece(worker1, new(posX, posY));

        // act
        var workerFound = board.GetWorker(playerName, 1);
        var workerNotFound = board.GetWorker(playerName, 2);

        // assert
        workerFound.Should().NotBeNull();
        workerFound.Number.Should().Be(1);
        workerFound.Player.Equals(player).Should().BeTrue();
        workerNotFound.Should().BeNull();
    }

    [Fact]
    public void Board_can_find_land_from_valid_coordinate()
    {
        // arrange
        var board = new Island();
        var posX = _faker.Random.Number(0, 4);
        var posY = _faker.Random.Number(0, 4);

        // act
        var success = board.TryGetLand(new(posX, posY), out var land);

        // assert
        success.Should().BeTrue();
        land.Should().NotBeNull();
        land.Coordinate.X.Should().Be(posX);
        land.Coordinate.Y.Should().Be(posY);
    }
}