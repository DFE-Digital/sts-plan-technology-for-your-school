using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public abstract class QuestionsViewBuilderTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string SectionSlug = "section-slug";
    protected const string SectionId = "section-id";
    protected const string SectionName = "Cyber security processes";
    protected const string FirstQuestionId = "question-1-id";
    protected const string FirstQuestionSlug = "question-1-slug";
    protected const string SecondQuestionId = "question-2-id";
    protected const string SecondQuestionSlug = "question-2-slug";
    protected const string ContactLinkId = "contact-link-id";
    protected const string ConcurrentUsersErrorMessage = "There was a problem, please contact us.";
    protected const int FocusedEstablishmentId = 22;

    protected readonly FakeLogger<QuestionsViewBuilder> _logger = new();
    protected readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    protected readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    protected readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    protected readonly IQuestionService _questionService = Substitute.For<IQuestionService>();
    protected readonly IRecommendationService _recommendationService =
        Substitute.For<IRecommendationService>();
    protected readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();

    protected readonly TestController _controller = new();

    protected readonly QuestionnaireQuestionEntry _firstQuestion = new()
    {
        Sys = new SystemDetails(FirstQuestionId),
        Slug = FirstQuestionSlug,
        Text = "Question one",
    };

    protected readonly QuestionnaireQuestionEntry _secondQuestion = new()
    {
        Sys = new SystemDetails(SecondQuestionId),
        Slug = SecondQuestionSlug,
        Text = "Question two",
    };

    protected readonly QuestionnaireSectionEntry _section;

    protected readonly SelectedEstablishmentDataModel _focusedEstablishment =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(FocusedEstablishmentId)
            .WithName("Focused School")
            .WithUrn("200022")
            .Build();

    protected QuestionsViewBuilderTestBase()
    {
        _section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(SectionId),
            Name = SectionName,
            Questions = [_firstQuestion, _secondQuestion],
        };

        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedEstablishment);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
    }

    protected QuestionsViewBuilder CreateSystemUnderTest(
        ContentfulOptionsConfiguration? contentfulOptions = null
    ) =>
        new(
            _logger,
            Options.Create(new ContactOptionsConfiguration { LinkId = ContactLinkId }),
            Options.Create(
                new ErrorMessagesConfiguration
                {
                    ConcurrentUsersOrContentChange = ConcurrentUsersErrorMessage,
                }
            ),
            contentfulOptions ?? new ContentfulOptionsConfiguration { UsePreviewApi = false },
            _contentfulService,
            _currentUserProvider,
            _establishmentService,
            _questionService,
            _recommendationService,
            _submissionService
        );

    protected sealed class TestController : Controller
    {
        public TestController()
        {
            var httpContext = new HttpContextBuilder().Build();
            ControllerContext = new ControllerContext { HttpContext = httpContext };
            TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
        }
    }
}
