using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Dfe.PlanTech.Data.Sql.Repositories;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Repositories;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Application.Workflows;

public class ResponseWorkflow
(
    SectionWorkflow sectionEntryRepository,
    SubmissionRepository submissionRepository
)
{
    private readonly SectionWorkflow _sectionEntryRepository = sectionEntryRepository ?? throw new ArgumentNullException(nameof(sectionEntryRepository));
    private readonly SubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<QuestionWithAnswerModel?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId)
    {
        return await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompleted: false, includeRelationships: true)
            .SelectMany(submission => submission.Responses)
            .Where(response => string.Equals(questionId, response.Question.ContentfulSysId))
            .OrderByDescending(response => response.DateCreated)
            .Select(response => new QuestionWithAnswerModel(response.AsDto()))
            .FirstOrDefaultAsync();
    }

    public async Task<SubmissionResponsesModel?> GetLatestResponsesForJourney(int establishmentId, string sectionId, bool isCompletedSubmission)
    {
        var latestSubmission = await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompletedSubmission, includeRelationships: true)
            .Select(submission => new SubmissionResponsesModel(submission.AsDto()))
            .FirstOrDefaultAsync();

        return latestSubmission is not null && latestSubmission.HasResponses
            ? latestSubmission
            : null;
    }

    public async Task<IEnumerable<QuestionWithAnswerModel>> GetOrderedResponsesForJourney(int establishmentId, string sectionId)
    {
        var submission = _submissionRepository. (establishmentId, sectionId);

        var responses = _res

        var questionAnswerModelMap = responses
                .Where(r => !string.IsNullOrWhiteSpace(r.QuestionSysId))
                .GroupBy(r => r.QuestionSysId)
                .ToDictionary(g => g.Key, g => g.First());

        var orderedResponses = new List<QuestionWithAnswerModel>();
        var section = await _sectionEntryRepository.GetSectionById(sectionId);
        var currentQuestion = section?.Questions.FirstOrDefault();

        while (currentQuestion is not null)
        {
            var questionSysId = currentQuestion.Sys.Id ?? string.Empty;
            if (!questionAnswerModelMap.TryGetValue(questionSysId, out var questionWithAnswerModel))
            {
                break;
            }

            var answer = GetAnswerForRef(section!.Questions, questionWithAnswerModel);

            // Show the latest Text and Slug, but preserve user answer if there has been a change
            questionWithAnswerModel = questionWithAnswerModel with
            {
                AnswerText = answer?.Text ?? questionWithAnswerModel.AnswerText,
                QuestionText = currentQuestion.Text,
                QuestionSlug = currentQuestion.Slug
            };

            orderedResponses.Add(questionWithAnswerModel);

            currentQuestion = answer?.NextQuestion;
        }

        return orderedResponses;
    }

    private AnswerEntry? GetAnswerForRef(List<QuestionEntry> questions, QuestionWithAnswerModel questionWithAnswerModel)
    {
        return questions
            .Find(question => string.Equals(question.Sys.Id, questionWithAnswerModel.QuestionSysId))?
            .Answers
            .Find(answer => string.Equals(answer.Sys.Id, questionWithAnswerModel.AnswerSysId));
    }
}
