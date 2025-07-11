using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Application.Workflows;

namespace Dfe.PlanTech.Application.Services;

public class QuestionService(
    ContentfulWorkflow contentfulWorkflow,
    ResponseWorkflow responseWorkflow
)
{
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
    private readonly ResponseWorkflow _responseWorkflow = responseWorkflow ?? throw new ArgumentNullException(nameof(responseWorkflow));

    public async Task<CmsQuestionnaireQuestionDto?> GetNextUnansweredQuestion(int establishmentId, string sectionId)
    {
        var section = await _contentfulWorkflow.GetEntryById<SectionEntry, CmsQuestionnaireSectionDto>(sectionId);

        var answeredQuestions = await _responseWorkflow.GetLatestResponses(establishmentId, section.Sys.Id, isCompletedSubmission: false);
        if (answeredQuestions is null)
            return section.Questions.FirstOrDefault();

        if (answeredQuestions.Responses.Count == 0)
            throw new DatabaseException($"There are no responses in the database for ongoing submission {answeredQuestions.SubmissionId}, linked to establishment {establishmentId}");

        return GetValidatedNextUnansweredQuestion(section, answeredQuestions);
    }

    /// <summary>
    /// Uses answered questions to find the next. If it is not possible to order user responses against the current questions,
    /// this indicates that content has changed or another user finished the submission concurrently.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="answeredQuestions"></param>
    /// <returns></returns>
    /// <exception cref="DatabaseException"></exception>
    private static CmsQuestionnaireQuestionDto? GetValidatedNextUnansweredQuestion(CmsQuestionnaireSectionDto section, SubmissionResponsesModel answeredQuestions)
    {
        var lastAttachedResponse = section.GetOrderedResponsesForJourney(answeredQuestions.Responses).LastOrDefault();

        if (lastAttachedResponse == null)
            throw new DatabaseException($"The responses to the ongoing submission {answeredQuestions.SubmissionId} are out of sync with the topic");

        return section.Questions.Where(question => question.Sys.Id == lastAttachedResponse.QuestionSysId)
                              .SelectMany(question => question.Answers)
                              .Where(answer => answer.Sys.Id == lastAttachedResponse.AnswerSysId)
                              .Select(answer => answer.NextQuestion)
                              .FirstOrDefault();
    }
}
