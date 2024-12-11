using Dfe.PlanTech.Domain.Database;

namespace Dfe.PlanTech.Domain.UnitTests.Database;

public class DatabaseRetryOptionsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5000)]
    [InlineData(10, 10000)]
    [InlineData(15, 15000)]
    public void DatabaseRetryOptions_Should_Set_Properties(int maxRetryCount, int maxDelayInMilliseconds)
    {
        var options = new DatabaseOptions(maxRetryCount, maxDelayInMilliseconds);

        Assert.Equal(maxRetryCount, options.MaxRetryCount);
        Assert.Equal(maxDelayInMilliseconds, options.MaxDelayInMilliseconds);
    }
}
