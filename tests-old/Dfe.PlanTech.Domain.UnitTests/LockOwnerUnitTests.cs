using Dfe.PlanTech.Domain.Caching.Models;

namespace Dfe.PlanTech.Domain.UnitTests;

public class LockOwnerUnitTests
{
    [Fact]
    public void Creates_And_Assigns_Properties()
    {
        var lockOwner = LockOwner.Create();

        Assert.NotNull(lockOwner.Id);
        Assert.Equal(lockOwner.Id, lockOwner.ToString());
    }
}
