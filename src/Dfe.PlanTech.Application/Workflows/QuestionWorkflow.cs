using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;
using static System.Collections.Specialized.BitVector32;

namespace Dfe.PlanTech.Application.Workflows
{
    public class QuestionWorkflow
    {
        private readonly ResponseRepository _responseRepository;
        private readonly SubmissionRepository _submissionRepository;

        public QuestionWorkflow(
            ResponseRepository responseRepository,
            SubmissionRepository submissionRepository
        )
        {
            _responseRepository = responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        }

        public async Task<CmsQuestionDto?> GetNextUnansweredQuestion(int establishmentId, CmsQuestionnaireSectionDto section)
        {
            var answeredQuestions = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, section.Sys.Id, false);
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
        private static Question? GetValidatedNextUnansweredQuestion(Section section, SubmissionResponsesDto answeredQuestions)
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
