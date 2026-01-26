using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

/// <summary>
/// Integration tests specifically focused on validating stored procedure calls from repository classes.
/// These tests ensure that stored procedures are called with correct parameters, types, and order.
/// </summary>
public class StoredProcedureCallValidationTests : DatabaseIntegrationTestBase
{
    private StoredProcedureRepository _storedProcRepository = null!;
    private SubmissionRepository _submissionRepository = null!;

    public StoredProcedureCallValidationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _storedProcRepository = new StoredProcedureRepository(DbContext);
        _submissionRepository = new SubmissionRepository(DbContext);
    }

    [Fact]
    public async Task StoredProcedureRepository_SetMaturityForSubmissionAsync_WhenCalledWithValidSubmissionId_ThenReturnsNonNegativeResult()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "TEST001", OrgName = "Test School" };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var submission = new SubmissionEntity
        {
            SectionId = "test-section",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = Core.Enums.SubmissionStatus.InProgress
        };
        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        // Act & Assert - Should execute without parameter type errors
        var result = await _storedProcRepository.SetMaturityForSubmissionAsync(submission.Id);

        // Validate the stored procedure was called successfully
        Assert.True(result >= 0, "Stored procedure should execute without errors");
    }

    [Fact]
    public async Task StoredProcedureRepository_SubmitResponse_WhenCalledWithValidResponseModel_ThenCreatesResponseAndReturnsId()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "test-user" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "TEST001", OrgName = "Test School", GroupUid = null };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };
        var questionModel = new IdWithTextModel { Id = question.ContentfulRef, Text = question.QuestionText };
        var answerModel = new IdWithTextModel { Id = answer.ContentfulRef, Text = answer.AnswerText };

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var submitAnswerModel = new SubmitAnswerModel
        {
            SectionId = "test-section",
            SectionName = "Test Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };

        // When user is logged in directly as a school (not MAT), both IDs are the same
        var assessmentResponse = new AssessmentResponseModel(user.Id, establishment.Id, establishment.Id, submitAnswerModel);

        // Act & Assert - Should execute without parameter type or order errors
        var responseId = await _storedProcRepository.SubmitResponse(assessmentResponse);

        // Validate the stored procedure was called successfully
        Assert.True(responseId > 0, "Response ID should be returned from stored procedure");

        // Verify the response was actually created
        var savedResponse = await DbContext.Responses.FindAsync(responseId);
        Assert.NotNull(savedResponse);
        Assert.Equal(user.Id, savedResponse!.UserId);
        Assert.Equal(establishment.Id, savedResponse.UserEstablishmentId);
        Assert.Equal(question.Id, savedResponse.QuestionId);
        Assert.Equal(answer.Id, savedResponse.AnswerId);
        Assert.Equal("", savedResponse.Maturity);
    }

    [Fact]
    public async Task StoredProcedureRepository_MultipleCalls_WhenParameterOrderValidated_ThenAllCallsExecuteWithoutParameterErrors()
    {
        // This test validates that stored procedure calls don't fail due to parameter order issues
        // by calling multiple stored procedures in sequence

        // Arrange
        var user = new UserEntity { DfeSignInRef = "param-test-user" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "PARAM001", OrgName = "Parameter Test School" };
        var question = new QuestionEntity { QuestionText = "Parameter Test Question", ContentfulRef = "PQ1" };
        var answer = new AnswerEntity { AnswerText = "Parameter Test Answer", ContentfulRef = "PA1" };
        var questionModel = new IdWithTextModel { Id = question.ContentfulRef, Text = question.QuestionText };
        var answerModel = new IdWithTextModel { Id = answer.ContentfulRef, Text = answer.AnswerText };

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        // Act & Assert - All stored procedure calls should succeed without parameter errors

        // 1. Test SubmitResponse parameter order
        var submitAnswerModel = new SubmitAnswerModel
        {
            SectionId = "param-section",
            SectionName = "Parameter Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };
        // When user is logged in directly as a school (not MAT), both IDs are the same
        var assessmentResponse = new AssessmentResponseModel(user.Id, establishment.Id, establishment.Id, submitAnswerModel);
        var responseId = await _storedProcRepository.SubmitResponse(assessmentResponse);
        Assert.True(responseId > 0);

        // 2. Test SetMaturityForSubmission parameter order
        var submission = new SubmissionEntity
        {
            SectionId = "param-section",
            SectionName = "Parameter Section",
            EstablishmentId = establishment.Id,
            Status = Core.Enums.SubmissionStatus.InProgress
        };
        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        var maturityResult = await _storedProcRepository.SetMaturityForSubmissionAsync(submission.Id);
        Assert.True(maturityResult >= 0);

        // If we reach here, all stored procedure calls succeeded without parameter order/type errors
        Assert.True(true, "All stored procedure calls completed without parameter errors");
    }
}
