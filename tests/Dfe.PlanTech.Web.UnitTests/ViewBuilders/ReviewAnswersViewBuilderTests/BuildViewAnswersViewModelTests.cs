using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.ViewBuilders;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.ReviewAnswersViewBuilderTests;

public class BuildViewAnswersViewModelTests
{
    private const string CategorySlug = "category-slug";
    private const string SectionSlug = "section-slug";
    private const string SectionName = "Section Name";
    private const string SchoolName = "Springfield Primary";

    private static readonly DateTime CompletedDate = new(2025, 3, 20, 10, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime CreatedDate = new(2025, 3, 1, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void BuildViewAnswersViewModel_WhenResponsesMatchSectionQuestions_ReturnsResponsesInSectionQuestionOrder()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Name = SectionName,
            Questions =
            [
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-1") },
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-2") },
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-3") },
            ],
        };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(
                1,
                [
                    new QuestionWithAnswerModel { QuestionSysId = "question-3" },
                    new QuestionWithAnswerModel { QuestionSysId = "question-1" },
                    new QuestionWithAnswerModel { QuestionSysId = "question-2" },
                ]
            ),
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Equal(
            ["question-1", "question-2", "question-3"],
            result.Responses.Select(response => response.QuestionSysId).ToArray()
        );
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenResponsesAreForRetiredQuestions_ReturnsResponsesInResponseOrder()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Name = SectionName,
            Questions = [new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-1") }],
        };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(
                1,
                [
                    new QuestionWithAnswerModel { QuestionSysId = "retired-1", Order = 1 },
                    new QuestionWithAnswerModel { QuestionSysId = "retired-3", Order = 3 },
                    new QuestionWithAnswerModel { QuestionSysId = "retired-2", Order = 2 },
                ]
            ),
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Equal(
            ["retired-1", "retired-2", "retired-3"],
            result.Responses.Select(response => response.QuestionSysId).ToArray()
        );
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenResponsesAreCoreAndRetired_ReturnsCoreResponsesFirstWithoutDuplicates()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Name = SectionName,
            Questions =
            [
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-1") },
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-2") },
            ],
        };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(
                1,
                [
                    new QuestionWithAnswerModel { QuestionSysId = "retired-1", Order = 1 },
                    new QuestionWithAnswerModel { QuestionSysId = "question-2", Order = 2 },
                    new QuestionWithAnswerModel { QuestionSysId = "question-1", Order = 3 },
                ]
            ),
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Equal(
            ["question-1", "question-2", "retired-1"],
            result.Responses.Select(response => response.QuestionSysId).ToArray()
        );
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenSectionQuestionHasNoSys_SkipsQuestionButCountsItInTotal()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Name = SectionName,
            Questions =
            [
                new QuestionnaireQuestionEntry(),
                new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-2") },
            ],
        };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(
                1,
                [new QuestionWithAnswerModel { QuestionSysId = "question-2" }]
            ),
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var response = Assert.Single(result.Responses);
        Assert.Equal("question-2", response.QuestionSysId);
        Assert.Equal(1, result.QuestionsAnswered);
        Assert.Equal(2, result.TotalQuestions);
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenSubmissionIsNull_ReturnsNoResponses()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry
        {
            Name = SectionName,
            Questions = [new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-1") }],
        };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            null,
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Empty(result.Responses);
        Assert.Equal(0, result.QuestionsAnswered);
        Assert.Equal(1, result.TotalQuestions);
        Assert.Null(result.DateStarted);
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenSubmissionHasDates_SetsCompletedAndStartedDates()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry { Name = SectionName };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(1, [])
            {
                DateCompleted = CompletedDate,
                DateCreated = CreatedDate,
            },
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Equal(CompletedDate, result.AssessmentCompletedDate);
        Assert.Equal(CreatedDate, result.DateStarted);
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenSubmissionHasNoCompletedDate_SetsFallbackCompletedDate()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry { Name = SectionName };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(1, []),
            SubmissionStatus.InProgress
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.NotEqual(default, result.AssessmentCompletedDate);
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenNotMatInProgressView_ReturnsRecommendationsNavigation()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry { Name = SectionName };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(1, []) { DateCompleted = CompletedDate },
            SubmissionStatus.CompleteReviewed
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug,
            schoolName: SchoolName
        );

        // Assert
        Assert.Equal(SectionName, result.TopicName);
        Assert.Equal(CategorySlug, result.CategorySlug);
        Assert.Equal(SectionSlug, result.SectionSlug);
        Assert.False(result.IsMatInProgressView);
        Assert.False(result.ShowInProgressDisclaimer);
        Assert.Equal($"/{CategorySlug}", result.BackLinkHref);
        Assert.Equal("Back to recommendations", result.BackButtonText);
    }

    [Fact]
    public void BuildViewAnswersViewModel_WhenMatInProgressView_ReturnsInProgressViewDetails()
    {
        // Arrange
        var section = new QuestionnaireSectionEntry { Name = SectionName };
        var submissionModel = new SubmissionRoutingDataModel(
            null,
            section,
            new SubmissionResponsesModel(1, []) { DateCreated = CreatedDate },
            SubmissionStatus.InProgress
        );

        // Act
        var result = ReviewAnswersViewBuilder.BuildViewAnswersViewModel(
            section,
            submissionModel,
            CategorySlug,
            SectionSlug,
            isMatInProgressView: true,
            schoolName: SchoolName
        );

        // Assert
        Assert.True(result.IsMatInProgressView);
        Assert.True(result.ShowInProgressDisclaimer);
        Assert.Equal(SchoolName, result.SchoolName);
        Assert.Equal(SchoolName, result.StartedBySchoolName);
        Assert.Equal($"/{CategorySlug}/{SectionSlug}/self-assessment", result.BackLinkHref);
        Assert.Equal("Back to section name", result.BackButtonText);
    }
}
