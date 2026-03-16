using System.Diagnostics.CodeAnalysis;
using Bogus;
using FluentAssertions;
using Santorini.Board;
using Santorini.Commands;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Santorini.UnitTests;

[ExcludeFromCodeCoverage]
public class MoveCommandTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_valid_command()
    {
        // arrange
        var playerName = _faker.Name.FirstName();
        var moveTo = new Coordinate
        (
            _faker.Random.Number(0, 4),
            _faker.Random.Number(0, 4)
        );
        var buildAt = default(Coordinate);
        while (buildAt is null || buildAt.Equals(moveTo))
            buildAt = new Coordinate
            (
                _faker.Random.Number(0, 4),
                _faker.Random.Number(0, 4)
            );

        // act
        var cmd = new MoveCommand(playerName, 1, moveTo, buildAt);

        // assert
        cmd.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Command_without_name_is_invalid()
    {
        // arrange
        var playerName = string.Empty;
        var moveTo = new Coordinate(0, 0);
        var buildAt = new Coordinate(0, 1);

        // act
        var cmd = new MoveCommand(playerName, 1, moveTo, buildAt);

        // assert
        cmd.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Command_without_moveto_coord_is_invalid()
    {
        // arrange
        var playerName = _faker.Name.FirstName();
        var moveTo = default(Coordinate);
        var buildAt = new Coordinate(0, 1);

        // act
        Action act = () => _ = new MoveCommand(playerName, 1, moveTo!, buildAt);

        // assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage($"*{nameof(moveTo)}*");
    }

    [Fact]
    public void Command_without_buildat_coord_is_invalid()
    {
        // arrange
        var playerName = _faker.Name.FirstName();
        var moveTo = new Coordinate(0, 0);
        var buildAt = default(Coordinate);

        // act
        Action act = () => _ = new MoveCommand(playerName, 1, moveTo, buildAt!);

        // assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage($"*{nameof(buildAt)}*");
    }

    [Fact]
    public void Command_with_same_moveto_and_buildat_is_invalid()
    {
        // arrange
        var playerName = _faker.Name.FirstName();
        var moveTo = new Coordinate(0, 0);
        var buildAt = new Coordinate(0, 0);

        // act
        var cmd = new MoveCommand(playerName, 1, moveTo, buildAt);

        // assert
        cmd.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Command_with_wrong_worker_number_is_invalid()
    {
        // arrange
        var playerName = _faker.Name.FirstName();
        var moveTo = new Coordinate(0, 0);
        var buildAt = new Coordinate(0, 0);

        // act
        var cmd1 = new MoveCommand(playerName, 0, moveTo, buildAt);
        var cmd2 = new MoveCommand(playerName, 3, moveTo, buildAt);

        // assert
        cmd1.IsValid.Should().BeFalse();
        cmd2.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Coord_can_comparer_themselves()
    {
        // arrange
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 0);
        var coord3 = new Coordinate(0, 1);
        var coord4 = new Coordinate(1, 0);
        var coord5 = default(Coordinate);

        // act
        var c1e2 = coord1.Equals(coord2);
        var c2e3 = coord1.Equals(coord3);
        var c3e4 = coord1.Equals(coord4);
        var c4e5 = coord1.Equals(coord5);


        // assert
        c1e2.Should().BeTrue();
        c2e3.Should().BeFalse();
        c3e4.Should().BeFalse();
        c4e5.Should().BeFalse();
    }
}