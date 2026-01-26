using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class StoredProcedureRepositoryTests : DatabaseIntegrationTestBase
{
    private StoredProcedureRepository _repository = null!;

    public StoredProcedureRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new StoredProcedureRepository(DbContext);
    }

    [Fact]
    public async Task StoredProcedureRepository_SubmitResponse_WhenGivenValidResponseModel_ThenReturnsResponseId()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "user123" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
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
            SectionId = "section-1",
            SectionName = "Test Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };

        // When user is logged in directly as a school (not MAT), both IDs are the same
        var response = new AssessmentResponseModel(user.Id, establishment.Id, establishment.Id, submitAnswerModel);

        var result = await _repository.SubmitResponse(response);
        Assert.True(result > 0);

        // Verify the response was actually created in the database
        var savedResponse = await DbContext.Responses.FirstOrDefaultAsync(r => r.Id == result);
        Assert.NotNull(savedResponse);
        Assert.Equal(user.Id, savedResponse!.UserId);
        Assert.Equal(establishment.Id, savedResponse.UserEstablishmentId);
    }

    [Fact]
    public async Task StoredProcedureRepository_SubmitResponse_WhenFirstResponseForSection_ThenSubmissionCreatedWithInProgressStatus()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "first-response-user" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "FIRST001", OrgName = "First Response School", GroupUid = null };
        var question = new QuestionEntity { QuestionText = "First Response Question", ContentfulRef = "FRQ1" };
        var answer = new AnswerEntity { AnswerText = "First Response Answer", ContentfulRef = "FRA1" };
        var questionModel = new IdWithTextModel { Id = question.ContentfulRef, Text = question.QuestionText };
        var answerModel = new IdWithTextModel { Id = answer.ContentfulRef, Text = answer.AnswerText };

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var submitAnswerModel = new SubmitAnswerModel
        {
            SectionId = "first-response-section",
            SectionName = "First Response Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };

        var assessmentResponse = new AssessmentResponseModel(user.Id, establishment.Id, establishment.Id, submitAnswerModel);

        // Act - Submit the first response for this section
        var responseId = await _repository.SubmitResponse(assessmentResponse);

        // Assert
        Assert.True(responseId > 0, "Response ID should be returned from stored procedure");

        // Clear EF cache to force fresh database query after stored procedure execution
        DbContext.ChangeTracker.Clear();

        // Verify the submission was created and is marked as InProgress
        var savedResponse = await DbContext.Responses.AsNoTracking().Include(r => r.Submission).FirstOrDefaultAsync(r => r.Id == responseId);
        Assert.NotNull(savedResponse);
        Assert.NotNull(savedResponse!.Submission);
        Assert.Equal(SubmissionStatus.InProgress, savedResponse.Submission!.Status);
    }

    [Fact]
    public async Task StoredProcedureRepository_SubmitResponse_WhenUserAndActiveEstablishmentDiffer_ThenUsesCorrectEstablishmentId()
    {
        // Arrange - A group user submitting a response for an establishment different from their own
        var schoolUser = new UserEntity { DfeSignInRef = "school-user" };
        var activeSchoolEstablishment = new EstablishmentEntity { EstablishmentRef = "USER003", OrgName = "Group Establishment" };

        var groupUser = new UserEntity { DfeSignInRef = "group-user" };
        var groupEstablishment = new EstablishmentEntity { EstablishmentRef = "USER002", OrgName = "User Establishment" };

        var question = new QuestionEntity { QuestionText = "Estab Diff Question", ContentfulRef = "EDQ1" };
        var answer = new AnswerEntity { AnswerText = "Estab Diff Answer", ContentfulRef = "EDA1" };
        var questionModel = new IdWithTextModel { Id = question.ContentfulRef, Text = question.QuestionText };
        var answerModel = new IdWithTextModel { Id = answer.ContentfulRef, Text = answer.AnswerText };

        DbContext.Users.AddRange(groupUser, schoolUser);
        DbContext.Establishments.AddRange(groupEstablishment, activeSchoolEstablishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var submitAnswerModel = new SubmitAnswerModel
        {
            SectionId = "estab-diff-section",
            SectionName = "Estab Diff Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };
        var assessmentResponse = new AssessmentResponseModel(groupUser.Id, activeSchoolEstablishment.Id, groupEstablishment.Id, submitAnswerModel);

        // Act - Create a new "submission" by submitting a first response
        var responseId = await _repository.SubmitResponse(assessmentResponse);

        // Assert
        // The submission created by submitting a response should be linked to the active establishment, not the user's establishment
        Assert.Equal(1, await DbContext.Submissions.CountAsync());
        var savedSubmission = await DbContext.Submissions.FirstOrDefaultAsync();
        Assert.NotNull(savedSubmission);
        Assert.Equal(activeSchoolEstablishment.Id, savedSubmission!.EstablishmentId);
        Assert.NotEqual(groupEstablishment.Id, savedSubmission.EstablishmentId);
    }

    [Fact]
    public async Task StoredProcedureRepository_SubmitResponse_WhenMultipleUsersSubmitResponses_ThenSubmissionEstablishmentIdStaysStatic()
    {
        // Arrange - A group user submitting a response for an establishment different from their own
        var schoolUser = new UserEntity { DfeSignInRef = "school-user" };
        var activeSchoolEstablishment = new EstablishmentEntity { EstablishmentRef = "USER003", OrgName = "Group Establishment" };

        var groupUser = new UserEntity { DfeSignInRef = "group-user" };
        var groupEstablishment = new EstablishmentEntity { EstablishmentRef = "USER002", OrgName = "User Establishment" };

        var question = new QuestionEntity { QuestionText = "Estab Diff Question", ContentfulRef = "EDQ1" };
        var answer = new AnswerEntity { AnswerText = "Estab Diff Answer", ContentfulRef = "EDA1" };
        var questionModel = new IdWithTextModel { Id = question.ContentfulRef, Text = question.QuestionText };
        var answerModel = new IdWithTextModel { Id = answer.ContentfulRef, Text = answer.AnswerText };

        DbContext.Users.AddRange(groupUser, schoolUser);
        DbContext.Establishments.AddRange(groupEstablishment, activeSchoolEstablishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        // Act & Assert (1) - Group user submits first response (creating the "submission"),
        // the "response" establishment ID is the active establishment ID
        var submitAnswerModel1 = new SubmitAnswerModel
        {
            SectionId = "estab-diff-section",
            SectionName = "Estab Diff Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };
        var assessmentResponse1 = new AssessmentResponseModel(groupUser.Id, activeSchoolEstablishment.Id, groupEstablishment.Id, submitAnswerModel1);
        _ = await _repository.SubmitResponse(assessmentResponse1);

        Assert.Equal(1, await DbContext.Responses.CountAsync());

        // The submission created by submitting a response should be linked to the active establishment, not the user's establishment
        Assert.Equal(1, await DbContext.Submissions.CountAsync());
        var savedSubmission = await DbContext.Submissions.FirstOrDefaultAsync();
        Assert.NotNull(savedSubmission);
        Assert.Equal(activeSchoolEstablishment.Id, savedSubmission!.EstablishmentId);
        Assert.NotEqual(groupEstablishment.Id, savedSubmission.EstablishmentId);

        // Act & Assert (2) - Now school user submits a response to the same section,
        // response establishment remains as the active school establishment's ID
        var submitAnswerModel2 = new SubmitAnswerModel
        {
            SectionId = "estab-diff-section",
            SectionName = "Estab Diff Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };
        var assessmentResponse2 = new AssessmentResponseModel(schoolUser.Id, activeSchoolEstablishment.Id, activeSchoolEstablishment.Id, submitAnswerModel2);
        _ = await _repository.SubmitResponse(assessmentResponse2);

        Assert.Equal(2, await DbContext.Responses.CountAsync());

        // The previous submission should be re-used/continued
        // The establishment ID should remain linked to the school and not the group user's establishment
        Assert.Equal(1, await DbContext.Submissions.CountAsync());
        var savedSubmission2 = await DbContext.Submissions.FirstOrDefaultAsync();
        Assert.NotNull(savedSubmission2);
        Assert.Equal(savedSubmission!.Id, savedSubmission2!.Id);
        Assert.Equal(activeSchoolEstablishment.Id, savedSubmission2.EstablishmentId);
        Assert.NotEqual(groupEstablishment.Id, savedSubmission2.EstablishmentId);

        // Act & Assert (3) - Group user submits another response to the same section,
        // response establishment remains as the active school establishment's ID
        var submitAnswerModel3 = new SubmitAnswerModel
        {
            SectionId = "estab-diff-section",
            SectionName = "Estab Diff Section",
            Question = questionModel,
            ChosenAnswer = answerModel
        };
        var assessmentResponse3 = new AssessmentResponseModel(groupUser.Id, activeSchoolEstablishment.Id, groupEstablishment.Id, submitAnswerModel3);
        _ = await _repository.SubmitResponse(assessmentResponse3);

        Assert.Equal(3, await DbContext.Responses.CountAsync());

        // The previous submission should be re-used/continued
        // The establishment ID should remain linked to the school and not the group user's establishment
        Assert.Equal(1, await DbContext.Submissions.CountAsync());
        var savedSubmission3 = await DbContext.Submissions.FirstOrDefaultAsync();
        Assert.NotNull(savedSubmission3);
        Assert.Equal(savedSubmission!.Id, savedSubmission3!.Id);
        Assert.Equal(activeSchoolEstablishment.Id, savedSubmission3.EstablishmentId);
        Assert.NotEqual(groupEstablishment.Id, savedSubmission3.EstablishmentId);
    }

    [Fact]
    public async Task StoredProcedureRepository_SetMaturityForSubmissionAsync_WhenCalledWithValidSubmissionId_ThenExecutesWithoutError()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var submission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = SubmissionStatus.InProgress
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        // Act & Assert - Should not throw
        var result = await _repository.SetMaturityForSubmissionAsync(submission.Id);

        // The stored procedure should execute successfully
        // The exact return value depends on the stored procedure implementation
        Assert.True(result >= 0);
    }
}
