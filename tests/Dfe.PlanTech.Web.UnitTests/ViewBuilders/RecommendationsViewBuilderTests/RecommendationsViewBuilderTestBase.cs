using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public abstract class RecommendationsViewBuilderTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string SectionSlug = "section-slug";
    protected const string FirstChunkSlug = "first-chunk";
    protected const string SecondChunkSlug = "second-chunk";
    protected const string ThirdChunkSlug = "third-chunk";
    protected const string SectionId = "section-id";
    protected const int FocusedEstablishmentId = 123;
    protected const int CurrentUserId = 42;
    protected const int UserEstablishmentId = 77;

    protected static readonly SelectedEstablishmentDataModel FocusedEstablishment =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(FocusedEstablishmentId)
            .WithName("Springfield Primary")
            .Build();

    protected readonly FakeLogger<RecommendationsViewBuilder> _logger = new();
    protected readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    protected readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    protected readonly INotifyService _notifyService = Substitute.For<INotifyService>();
    protected readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    protected readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    protected readonly IMicrocopyProvider _microcopyProvider = Substitute.For<IMicrocopyProvider>();
    protected readonly IGroupService _groupService = Substitute.For<IGroupService>();

    protected RecommendationsViewBuilder CreateSystemUnderTest() =>
        new(
            _logger,
            _contentfulService,
            _currentUserProvider,
            _notifyService,
            _recommendationService,
            _submissionService,
            _microcopyProvider,
            _groupService
        );

    protected void ArrangeContentful(
        QuestionnaireCategoryEntry? category,
        QuestionnaireSectionEntry? section,
        int? sectionIncludeLevel = null
    )
    {
        _contentfulService.GetCategoryBySlugAsync(CategorySlug).Returns(category);
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug, sectionIncludeLevel)
            .Returns(section!);
    }
}
