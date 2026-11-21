using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Tests.Shared.Assertions;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class ConstructorTests : CurrentUserProviderTestBase
{
    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<CurrentUserProvider>(
            _logger,
            _contextAccessor,
            _establishmentService,
            _giasService
        );
    }
}
