using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Dfe.PlanTech.Web.Workflows;

namespace Dfe.PlanTech.Web.Services
{
    public class QuestionService
    {
        private readonly ContentfulWorkflow _contentfulWorkflow;
        private readonly ResponseWorkflow _responseWorkflow;
        private readonly SubmissionWorkflow _submissionWorkflow;

        public QuestionService(
            ContentfulWorkflow contentfulWorkflow,
            ResponseWorkflow responseWorkflow,
            SubmissionWorkflow submissionWorkflow
        )
        {
            _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
            _responseWorkflow = responseWorkflow ?? throw new ArgumentNullException(nameof(responseWorkflow));
            _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));
        }

        public async Task<CmsQuestionDto?> GetNextUnansweredQuestion(int establishmentId, string sectionId)
        {
            var section = await _contentfulWorkflow.GetEntryById<SectionEntry, CmsSectionDto>(sectionId);

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
        private static CmsQuestionDto? GetValidatedNextUnansweredQuestion(CmsSectionDto section, SubmissionResponsesModel answeredQuestions)
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
}
