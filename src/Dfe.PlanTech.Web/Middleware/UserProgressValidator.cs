using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Exceptions;

namespace Dfe.PlanTech.Web.Middleware;

public class UserProgressValidator
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
    private readonly IUser _user;

    public UserProgressValidator(IGetSectionQuery getSectionQuery,
                                    IGetSubmissionStatusesQuery getSubmissionStatusesQuery,
                                    IUser user)
    {
        _getSectionQuery = getSectionQuery;
        _getSubmissionStatusesQuery = getSubmissionStatusesQuery;
        _user = user;
    }

    public async Task<JourneyStatusInfo> GetJourneyStatusForSection(string sectionSlug, CancellationToken cancellationToken)
    {
        var establishmentId = await _user.GetEstablishmentId();

        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                        throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var sectionStatus = await _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(establishmentId,
                                                                                              section,
                                                                                              cancellationToken);

        bool sectionStarted = sectionStatus?.SectionStatus != null && !sectionStatus.SectionStatus.Completed && sectionStatus.LatestResponse != null;

        if (!sectionStarted)
        {
            return new JourneyStatusInfo(JourneyStatus.NotStarted, section, section.Questions.First());
        }
        else if (sectionStatus!.SectionStatus!.Completed)
        {
            return new JourneyStatusInfo(JourneyStatus.Completed, section, null, null, sectionStatus.SectionStatus.Maturity);
        }

        if (sectionStatus.LatestResponse == null)
        {
            throw new InvalidDataException("Latest response is null, section has started");
        }

        var lastAnsweredQuestion = section.Questions.FirstOrDefault(question => question.Sys.Id == sectionStatus.LatestResponse.QuestionContentfulId) ??
                                        throw new InvalidDataException($"Could not find question {sectionStatus.LatestResponse.QuestionContentfulId} in section {section.Sys.Id}");

        var lastSelectedAnswer = lastAnsweredQuestion.Answers.FirstOrDefault(answer => answer.Sys.Id == sectionStatus.LatestResponse.AnswerContentfulId) ??
                                    throw new InvalidDataException($"Could not find answer {sectionStatus.LatestResponse.AnswerContentfulId} in question {sectionStatus.LatestResponse.QuestionContentfulId} in section {section.Sys.Id}");

        if (lastSelectedAnswer.NextQuestion == null)
        {
            return new JourneyStatusInfo(JourneyStatus.CheckAnswers, section);
        }

        return new JourneyStatusInfo(JourneyStatus.NextQuestion, section, lastSelectedAnswer.NextQuestion, sectionStatus.LatestResponse.AnswerContentfulId);
    }
}

public enum JourneyStatus
{
    NotStarted,
    NextQuestion,
    CheckAnswers,
    Completed
}

public record JourneyStatusInfo(JourneyStatus Status, Section Section, Question? NextQuestion = null, string? LastResponseAnswerContentfulId = null, string? Maturity = null);