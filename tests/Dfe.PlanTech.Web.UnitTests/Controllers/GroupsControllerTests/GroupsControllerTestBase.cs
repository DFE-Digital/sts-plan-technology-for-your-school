using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public abstract class GroupsControllerTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string SectionSlug = "section-slug";
    protected const string RecommendationSlug = "recommendation-slug";

    protected readonly FakeLogger<GroupsController> _logger = new();
    protected readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    protected readonly IGroupsViewBuilder _groupsViewBuilder = Substitute.For<IGroupsViewBuilder>();
    protected readonly IGroupSelectSchoolsToAssessValidator _groupSelectSchoolsToAssessValidator =
        Substitute.For<IGroupSelectSchoolsToAssessValidator>();
    protected readonly IGroupSelectSchoolsToUpdateStatusValidator _groupSelectSchoolsToUpdateStatusValidator =
        Substitute.For<IGroupSelectSchoolsToUpdateStatusValidator>();
    protected readonly IPagesViewBuilder _pagesViewBuilder = Substitute.For<IPagesViewBuilder>();
    protected readonly FakeSession _session = new();

    protected GroupsController CreateSystemUnderTest() =>
        new(
            _logger,
            _currentUserProvider,
            _groupsViewBuilder,
            _groupSelectSchoolsToAssessValidator,
            _groupSelectSchoolsToUpdateStatusValidator,
            _pagesViewBuilder
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new HttpContextBuilder().WithSession(_session).Build(),
                RouteData = new RouteData(),
            },
        };
}
