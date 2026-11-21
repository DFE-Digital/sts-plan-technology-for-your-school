using Dfe.PlanTech.Web.Controllers;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class ConstructorTests : GroupsControllerTestBase
{
    // ConstructorGuardAssert cannot be used: the logger parameter is passed to
    // BaseController, which does not null-guard it.
    [Fact]
    public void Constructor_WhenAnyGuardedDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var currentUserException = Assert.Throws<ArgumentNullException>(() =>
            new GroupsController(
                _logger,
                null!,
                _groupsViewBuilder,
                _groupSelectSchoolsToAssessValidator,
                _groupSelectSchoolsToUpdateStatusValidator,
                _pagesViewBuilder
            )
        );
        var groupsViewBuilderException = Assert.Throws<ArgumentNullException>(() =>
            new GroupsController(
                _logger,
                _currentUserProvider,
                null!,
                _groupSelectSchoolsToAssessValidator,
                _groupSelectSchoolsToUpdateStatusValidator,
                _pagesViewBuilder
            )
        );
        var assessValidatorException = Assert.Throws<ArgumentNullException>(() =>
            new GroupsController(
                _logger,
                _currentUserProvider,
                _groupsViewBuilder,
                null!,
                _groupSelectSchoolsToUpdateStatusValidator,
                _pagesViewBuilder
            )
        );
        var updateStatusValidatorException = Assert.Throws<ArgumentNullException>(() =>
            new GroupsController(
                _logger,
                _currentUserProvider,
                _groupsViewBuilder,
                _groupSelectSchoolsToAssessValidator,
                null!,
                _pagesViewBuilder
            )
        );
        var pagesViewBuilderException = Assert.Throws<ArgumentNullException>(() =>
            new GroupsController(
                _logger,
                _currentUserProvider,
                _groupsViewBuilder,
                _groupSelectSchoolsToAssessValidator,
                _groupSelectSchoolsToUpdateStatusValidator,
                null!
            )
        );

        Assert.Equal("currentUser", currentUserException.ParamName);
        Assert.Equal("groupsViewBuilder", groupsViewBuilderException.ParamName);
        Assert.Equal("groupSelectSchoolsToAssessValidator", assessValidatorException.ParamName);
        Assert.Equal(
            "groupSelectSchoolsToUpdateStatusValidator",
            updateStatusValidatorException.ParamName
        );
        Assert.Equal("pagesViewBuilder", pagesViewBuilderException.ParamName);
    }
}
