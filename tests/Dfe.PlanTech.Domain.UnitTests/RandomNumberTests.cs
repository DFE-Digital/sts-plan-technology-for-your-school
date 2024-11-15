using Dfe.PlanTech.Domain.Helpers;

namespace Dfe.PlanTech.Domain.UnitTests;
public class RandomNumberTests
{
    [Fact]
    public void GenerateRandomInt_ValidRange_ReturnsValueWithinRange()
    {
        // Arrange
        int min = 1;
        int max = 10;
        int iterations = 1000; // Number of times to call the method for testing

        // Act
        bool isWithinRange = true;
        for (int i = 0; i < iterations; i++)
        {
            int result = RandomNumber.GenerateRandomInt(min, max);
            if (result < min || result >= max)
            {
                isWithinRange = false;
                break;
            }
        }

        // Assert
        Assert.True(isWithinRange, "Generated number is out of the specified range.");
    }

    [Fact]
    public void GenerateRandomInt_MinEqualsMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int min = 5;
        int max = 5;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RandomNumber.GenerateRandomInt(min, max));
        Assert.Equal("min", exception.ParamName);
        Assert.Contains("Minimum value must be less than maximum value.", exception.Message);
    }

    [Fact]
    public void GenerateRandomInt_MinGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int min = 10;
        int max = 5;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RandomNumber.GenerateRandomInt(min, max));
        Assert.Equal("min", exception.ParamName);
        Assert.Contains("Minimum value must be less than maximum value.", exception.Message);
    }
}
