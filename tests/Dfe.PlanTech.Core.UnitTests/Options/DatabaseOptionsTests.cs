using Dfe.PlanTech.Core.Options;

namespace Dfe.PlanTech.Core.UnitTests.Options;

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

    [Fact]
    public void DatabaseRetryOptions_Should_Use_Defaults_When_No_Parameters()
    {
        var options = new DatabaseOptions();
        Assert.Equal(5, options.MaxRetryCount);
        Assert.Equal(5000, options.MaxDelayInMilliseconds);
    }
}
