using System.Data;
using System.Security.Authentication;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Submissions.Commands;

public class SubmitAnswerCommand : ISubmitAnswerCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly IUser _user;

    public SubmitAnswerCommand(IPlanTechDbContext db, IUser user)
    {
        _db = db;
        _user = user;
    }

    //If so, return some sort of result indicating so
    //Then on the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    //Which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
    public async Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, CancellationToken cancellationToken = default)
    {
        if (submitAnswerDto?.ChosenAnswer == null)
            throw new InvalidDataException($"{nameof(submitAnswerDto.ChosenAnswer)} is null");

        int userId = await _user.GetCurrentUserId() ?? throw new AuthenticationException("User is not authenticated");
        int establishmentId = await _user.GetEstablishmentId();

        var dto = new RecordResponseDto()
        {
            SectionId = submitAnswerDto.SectionId,
            SectionName = submitAnswerDto.SectionName,
            Answer = submitAnswerDto.ChosenAnswer!.Answer,
            Question = new IdWithText()
            {
                Text = submitAnswerDto.QuestionText,
                Id = submitAnswerDto.QuestionId
            },
            Maturity = submitAnswerDto.ChosenAnswer.Maturity,
            EstablishmentId = establishmentId,
            UserId = userId
        };

        var responseId = await SubmitResponse(dto, cancellationToken);

        return responseId;
    }

    private async Task<int> SubmitResponse(RecordResponseDto recordResponseDto, CancellationToken cancellationToken)
    {
        var establishmentId = new SqlParameter("@establishmentId", recordResponseDto.EstablishmentId);
        var userId = new SqlParameter("@userId", recordResponseDto.UserId);
        var sectionId = new SqlParameter("@sectionId", recordResponseDto.SectionId);
        var sectionName = new SqlParameter("@sectionName", recordResponseDto.SectionName);
        var questionContentfulId = new SqlParameter("@questionContentfulId", recordResponseDto.Question.Id);
        var questionText = new SqlParameter("@questionText", recordResponseDto.Question.Text);
        var answerContentfulId = new SqlParameter("@answerContentfulId", recordResponseDto.Answer.Id);
        var answerText = new SqlParameter("@answerText", recordResponseDto.Answer.Text);
        var maturity = new SqlParameter("@maturity", recordResponseDto.Maturity);

        var responseId = new SqlParameter("@responseId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var submissionId = new SqlParameter("@submissionId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await _db.ExecuteSqlAsync($@"EXEC SubmitAnswer
                                            @establishmentId={establishmentId},
                                            @userId={userId},
                                            @sectionId={sectionId},
                                            @sectionName={sectionName},
                                            @questionContentfulId={questionContentfulId},
                                            @questionText={questionText},
                                            @answerContentfulId={answerContentfulId},
                                            @answerText={answerText},
                                            @maturity={maturity},
                                            @responseId={responseId} OUTPUT,
                                            @submissionId={submissionId} OUTPUT",
                                            cancellationToken);

        if (responseId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException($"responseId is not int - is {responseId.Value}");

    }
}
