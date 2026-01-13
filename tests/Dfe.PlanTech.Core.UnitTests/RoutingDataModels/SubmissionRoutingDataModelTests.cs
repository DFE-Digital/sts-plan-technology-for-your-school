using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;

namespace Dfe.PlanTech.Core.UnitTests.RoutingDataModels;

public class SubmissionRoutingDataModelTests
{
    #region Constants
    private const SubmissionStatus ArbitraryStatus = SubmissionStatus.InProgress;
    #endregion

    #region IsQuestionInResponses

    [Theory]
    [InlineData("111", true)]
    [InlineData("222", true)]
    [InlineData("333", true)]
    [InlineData("ans-111", false)]
    [InlineData("ans-222", false)]
    [InlineData("ans-333", false)]
    [InlineData("arbitrary-not-found-question-sys-id", false)]
    [InlineData("999", false)]
    public void IsQuestionInResponses_MatchesExpectation(string searchQuestionSysId, bool expected)
    {
        // Arrange: build a simplified set of responses to search against
        var questionnaireSection = new QuestionnaireSectionEntry();
        var responses = new List<SqlResponseDto>
            {
                new SqlResponseDto
                {
                    Question = new SqlQuestionDto {ContentfulSysId = "111"},
                    Answer = new SqlAnswerDto {ContentfulSysId = "ans-111"}
                },
                new SqlResponseDto
                {
                    Question = new SqlQuestionDto {ContentfulSysId = "222"},
                    Answer = new SqlAnswerDto {ContentfulSysId = "ans-222"}
                },
                new SqlResponseDto
                {
                    Question = new SqlQuestionDto {ContentfulSysId = "333"},
                    Answer = new SqlAnswerDto {ContentfulSysId = "ans-333"}
                }
            }
            .Select(dto => new QuestionWithAnswerModel(dto, questionnaireSection))
            .ToList();

        var submission = new SubmissionResponsesModel(
            new SqlSubmissionDto(),
            questionnaireSection
        )
        {
            Responses = responses
        };

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: questionnaireSection,
            submission: submission,
            status: ArbitraryStatus
        );

        // Act
        var result = routingData.IsQuestionInResponses(searchQuestionSysId);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion


    #region GetLatestResponseForQuestion

    [Fact]
    public void GetLatestResponseForQuestion_ReturnsResponse_WhenQuestionExists()
    {
        // Arrange
        var questionSysId = "123";
        var responses = new List<SqlResponseDto>
        {
            new SqlResponseDto
            {
                Question = new SqlQuestionDto {ContentfulSysId = questionSysId},
                Answer = new SqlAnswerDto {ContentfulSysId = "arbitrary"}
            }
        };
        var questionnaireSection = new QuestionnaireSectionEntry();
        var submission = new SubmissionResponsesModel(
            new SqlSubmissionDto
            {
                Responses = responses
            },
            questionnaireSection
        );
        var expectedResponse = submission.Responses[0];

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: questionnaireSection,
            submission: submission,
            status: ArbitraryStatus
        );

        // Act
        var result = routingData.GetLatestResponseForQuestion(questionSysId);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public void GetLatestResponseForQuestion_ThrowsException_WhenQuestionDoesNotExist()
    {
        // Arrange
        var questionSysId = "123";
        var questionnaireSection = new QuestionnaireSectionEntry();
        var submission = new SubmissionResponsesModel(
            new SqlSubmissionDto
            {
                Responses = new List<SqlResponseDto>()
            },
            questionnaireSection
        );

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: questionnaireSection,
            submission: submission,
            status: ArbitraryStatus
        );

        // Act & Assert
        Assert.Throws<DatabaseException>(() => routingData.GetLatestResponseForQuestion(questionSysId));
    }

    #endregion

    #region GetQuestionForSlug

    [Fact]
    public void GetQuestionForSlug_ReturnsQuestion_WhenSlugExists()
    {
        // Arrange
        var slug = "test-slug";
        var expectedQuestion = new QuestionnaireQuestionEntry { Slug = slug };
        var questionnaireSection = new QuestionnaireSectionEntry
        {
            Questions = new List<QuestionnaireQuestionEntry> { expectedQuestion }
        };

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: questionnaireSection,
            submission: null,
            status: ArbitraryStatus
        );

        // Act
        var result = routingData.GetQuestionForSlug(slug);

        // Assert
        Assert.Equal(expectedQuestion, result);
    }

    [Fact]
    public void GetQuestionForSlug_ThrowsException_WhenSlugDoesNotExist()
    {
        // Arrange
        var slug = "test-slug";
        var questionnaireSection = new QuestionnaireSectionEntry
        {
            Questions = new List<QuestionnaireQuestionEntry>()
        };

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: questionnaireSection,
            submission: null,
            status: ArbitraryStatus
        );

        // Act & Assert
        Assert.Throws<ContentfulDataUnavailableException>(() => routingData.GetQuestionForSlug(slug));
    }

    #endregion

    #region Properties

    [Fact]
    public void SubmissionRoutingDataModel_Sets_Properties_Correctly()
    {
        // Arrange
        var questionnaireSection = new QuestionnaireSectionEntry();
        var nextQuestion = new QuestionnaireQuestionEntry { Slug = "arbitrary-next" };
        var submission = new SubmissionResponsesModel(
            new SqlSubmissionDto(),
            questionnaireSection
        );

        // Act
        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: nextQuestion,
            questionnaireSection: questionnaireSection,
            submission: submission,
            status: ArbitraryStatus
        );

        // Assert
        Assert.Equal(ArbitraryStatus, routingData.Status);
        Assert.Same(nextQuestion, routingData.NextQuestion);
        Assert.Same(questionnaireSection, routingData.QuestionnaireSection);
        Assert.Same(submission, routingData.Submission);
    }

    #endregion


    // TODO: Additional tests for edge cases, such as null or empty DTOs and empty/whitespace strings
}
