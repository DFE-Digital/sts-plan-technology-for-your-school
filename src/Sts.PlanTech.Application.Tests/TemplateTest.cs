namespace Sts.PlanTech.Application.Tests;

public class TemplateTest
{
    [Fact]
    public void Should_Do_Something_When_True()
    {
        // Arrange.
        int A = 1;
        int Expected = 3;

        // Act.
        int Actual = A + 2;

        // Assert.
        Assert.Equal(Expected, Actual);
    }
}
