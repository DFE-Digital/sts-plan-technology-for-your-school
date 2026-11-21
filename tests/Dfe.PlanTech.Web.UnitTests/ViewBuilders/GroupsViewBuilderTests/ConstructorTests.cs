using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class ConstructorTests : GroupsViewBuilderTestBase
{
    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GroupsViewBuilder>(
            _logger,
            Options.Create(new ContactOptionsConfiguration { LinkId = ContactLinkId }),
            _contentfulService,
            _currentUserProvider,
            _establishmentService,
            _groupService,
            _submissionService,
            _recommendationService,
            _microcopyProvider
        );
    }
}
