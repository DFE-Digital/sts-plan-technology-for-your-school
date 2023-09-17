using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Submissions.Commands;

public class SubmitAnswerCommand : ISubmitAnswerCommand
{
    private readonly ICreateResponseCommand _createResponseCommand;
    private readonly IUser _user;

    public SubmitAnswerCommand(ICreateResponseCommand createResponseCommand, IUser user)
    {
        _createResponseCommand = createResponseCommand;
        _user = user;
    }

    //If so, return some sort of result indicating so
    //Then on the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    //Which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
    public async Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, CancellationToken cancellationToken = default)
    {
        if (submitAnswerDto.ChosenAnswer == null) throw new NullReferenceException(nameof(submitAnswerDto.ChosenAnswer));

        //TODO: Change exception type
        int userId = await _user.GetCurrentUserId() ?? throw new Exception("User is not authenticated");
        int establishmentId = await _user.GetEstablishmentId();

        //TODO: Bring this functionality to this class - duplicate purpose
        var responseId = await _createResponseCommand.CreateResponse(new RecordResponseDto()
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
        });

        return responseId;
    }
}
