namespace Dfe.PlanTech.Application.UnitTests;

public class TemplateTest
{
    [Fact]
    public void Should_Do_Something_When_True()
    {
        // Arrange.
        int a = 1;
        int expected = 3;

        // Act.
        int actual = a + 2;

        // Assert.
        Assert.Equal(expected, actual);
    }
}
