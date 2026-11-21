using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.ReviewAnswersViewBuilderTests;

public abstract class ReviewAnswersViewBuilderTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string CategoryName = "Category Name";
    protected const string SectionSlug = "section-slug";
    protected const string SectionId = "section-id";
    protected const string SectionName = "Section Name";
    protected const int FocusedEstablishmentId = 11;

    protected readonly FakeLogger<ReviewAnswersViewBuilder> _logger = new();
    protected readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    protected readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    protected readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();

    protected ReviewAnswersViewBuilder CreateSystemUnderTest() =>
        new(_logger, _contentfulService, _currentUserProvider, _submissionService);

    protected void ArrangeFocusedEstablishment() =>
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );

    protected void ArrangeRouting(
        int establishmentId,
        QuestionnaireSectionEntry section,
        SubmissionStatus requestedStatus,
        SubmissionStatus routingStatus,
        SubmissionResponsesModel? submission = null,
        QuestionnaireQuestionEntry? nextQuestion = null
    ) =>
        _submissionService
            .GetSubmissionRoutingDataAsync(establishmentId, section, requestedStatus)
            .Returns(
                new SubmissionRoutingDataModel(nextQuestion, section, submission, routingStatus)
            );
}
