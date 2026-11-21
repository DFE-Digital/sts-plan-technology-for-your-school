using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public abstract class GroupsViewBuilderTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string SectionSlug = "section-slug";
    protected const string SectionId = "section-id";
    protected const string RecommendationSlug = "recommendation-slug";
    protected const string RecommendationChunkId = "recommendation-chunk-id";
    protected const string ContactLinkId = "contact-link-id";
    protected const string FirstQuestionSlug = "first-question-slug";

    protected readonly FakeLogger<GroupsViewBuilder> _logger = new();
    protected readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    protected readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    protected readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    protected readonly IGroupService _groupService = Substitute.For<IGroupService>();
    protected readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    protected readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    protected readonly IMicrocopyProvider _microcopyProvider = Substitute.For<IMicrocopyProvider>();

    protected readonly SelectedEstablishmentDataModel _oakfieldPrimary =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(11)
            .WithUrn("100011")
            .WithName("Oakfield Primary")
            .Build();

    protected readonly SelectedEstablishmentDataModel _riverbankPrimary =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(12)
            .WithUrn("100012")
            .WithName("Riverbank Primary")
            .Build();

    protected readonly QuestionnaireSectionEntry _section = new()
    {
        Sys = new SystemDetails(SectionId),
        Name = "Cyber security",
        Questions = [new QuestionnaireQuestionEntry { Slug = FirstQuestionSlug }],
        CoreRecommendations =
        [
            new RecommendationChunkEntry
            {
                Sys = new SystemDetails(RecommendationChunkId),
                Slug = RecommendationSlug,
            },
        ],
    };

    protected GroupsViewBuilder CreateSystemUnderTest() =>
        new(
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
