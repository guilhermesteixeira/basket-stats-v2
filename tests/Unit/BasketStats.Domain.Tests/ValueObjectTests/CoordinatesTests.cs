namespace BasketStats.Domain.Tests.ValueObjectTests;

using Domain.ValueObjects;

public class CoordinatesTests
{
    [Fact]
    public void Create_WithValidCoordinates_Succeeds()
    {
        // Arrange & Act
        var coordinates = new Coordinates(50, 75);

        // Assert
        Assert.Equal(50, coordinates.X);
        Assert.Equal(75, coordinates.Y);
    }

    [Fact]
    public void Create_WithZeroCoordinates_Succeeds()
    {
        // Arrange & Act
        var coordinates = new Coordinates(0, 0);

        // Assert
        Assert.Equal(0, coordinates.X);
        Assert.Equal(0, coordinates.Y);
    }

    [Fact]
    public void Create_WithMaxCoordinates_Succeeds()
    {
        // Arrange & Act
        var coordinates = new Coordinates(100, 100);

        // Assert
        Assert.Equal(100, coordinates.X);
        Assert.Equal(100, coordinates.Y);
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(101, 50)]
    [InlineData(50, -1)]
    [InlineData(50, 101)]
    public void Create_WithInvalidCoordinates_Throws(decimal x, decimal y)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Coordinates(x, y));
    }

    [Fact]
    public void TwoCoordinates_WithSameValues_AreEqual()
    {
        // Arrange
        var coord1 = new Coordinates(50, 75);
        var coord2 = new Coordinates(50, 75);

        // Act & Assert
        Assert.Equal(coord1, coord2);
    }

    [Fact]
    public void TwoCoordinates_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var coord1 = new Coordinates(50, 75);
        var coord2 = new Coordinates(50, 76);

        // Act & Assert
        Assert.NotEqual(coord1, coord2);
    }
}
