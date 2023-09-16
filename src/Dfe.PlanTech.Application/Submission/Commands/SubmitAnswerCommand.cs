using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Submission.Commands;

public class SubmitAnswerCommand : ISubmitAnswerCommand
{
    private readonly GetSubmitAnswerQueries _getSubmitAnswerQueries;
    private readonly ICreateResponseCommand _createResponseCommand;
    private readonly IUser _user;

    public SubmitAnswerCommand(ICreateResponseCommand createResponseCommand, IUser user, GetSubmitAnswerQueries getSubmitAnswerQueries)
    {
        _createResponseCommand = createResponseCommand;
        _user = user;
        _getSubmitAnswerQueries = getSubmitAnswerQueries;
    }

    //TODO: This should check if we are re-submitting an answer
    //If so, return some sort of result indicating so
    //Then on the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    //Which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
    public async Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName)
    {
        if (submitAnswerDto.ChosenAnswer == null) throw new NullReferenceException(nameof(submitAnswerDto.ChosenAnswer));

        int userId = await _getSubmitAnswerQueries.GetUserId();
        int establishmentId = await _user.GetEstablishmentId();

        //TODO: Bring this functionality to this class - duplicate purpose
        var responseId = await _createResponseCommand.CreateResponsNew(new RecordResponseDto()
        {
            SectionId = sectionId,
            SectionName = sectionName,
            Answer = submitAnswerDto.ChosenAnswer!.Answer,
            Question = new IdWithText()
            {
                Text = submitAnswerDto.QuestionText,
                Id = submitAnswerDto.QuestionId
            },
            Maturity = submitAnswerDto.ChosenAnswer.Maturity,
            EstablishmentId = establishmentId,
            UserId = userId
        });

        return responseId;
    }
}
