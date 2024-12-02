using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Responses.Queries;

public class GetLatestResponsesQuery(IPlanTechDbContext db) : IGetLatestResponsesQuery
{
    private readonly IPlanTechDbContext _db = db;

    public async Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default)
    {
        var responseListByDate = GetCurrentSubmission(establishmentId, sectionId, false)
                                        .SelectMany(submission => submission.Responses)
                                        .Where(response => response.Question.ContentfulRef == questionId)
                                        .OrderByDescending(response => response.DateCreated)
                                        .Select(ToQuestionWithAnswer());

        return await _db.FirstOrDefaultAsync(responseListByDate, cancellationToken);
    }

    public async Task<SubmissionResponsesDto?> GetLatestResponses(int establishmentId, string sectionId, bool completedSubmission, CancellationToken cancellationToken = default)
    {
        var latestSubmissionResponses = await _db.FirstOrDefaultAsync(GetLatestResponsesBySectionIdQueryable(establishmentId, sectionId, completedSubmission), cancellationToken);

        return HaveSubmission(latestSubmissionResponses) ? latestSubmissionResponses : null;
    }

    private static bool HaveSubmission(SubmissionResponsesDto? latestSubmissionResponses)
    => latestSubmissionResponses != null && latestSubmissionResponses.HaveAnyResponses;

    private IQueryable<SubmissionResponsesDto> GetLatestResponsesBySectionIdQueryable(int establishmentId, string sectionId, bool completedSubmission)
    => GetCurrentSubmission(establishmentId, sectionId, completedSubmission)
            .Select(submission => new SubmissionResponsesDto()
            {
                SubmissionId = submission.Id,
                Responses = submission.Responses.Select(response => new QuestionWithAnswer
                {
                    QuestionRef = response.Question.ContentfulRef,
                    QuestionText = response.Question.QuestionText ?? "", //Should this come from Contentful?
                    AnswerRef = response.Answer.ContentfulRef,
                    AnswerText = response.Answer.AnswerText ?? "",//Should this come from Contentful?
                    DateCreated = response.DateCreated
                })
                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionRef)
                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
                .ToList()
            });

    private IQueryable<Submission> GetCurrentSubmission(int establishmentId, string sectionId, bool completedSubmission)
    => _db.GetSubmissions
            .Where(IsMatchingIncompleteSubmission(establishmentId, sectionId, completedSubmission))
            .OrderByDescending(submission => submission.DateCreated);

    public async Task ViewLatestSubmission(int establishmentId, string sectionId)
    {
        var currentSubmission = await db.FirstOrDefaultAsync(GetCurrentSubmission(establishmentId, sectionId, true));
        if (currentSubmission != null)
        {
            currentSubmission.Viewed = true;
            await db.SaveChangesAsync();
        }
    }

    private static Expression<Func<Submission, bool>> IsMatchingIncompleteSubmission(int establishmentId, string sectionId, bool completedSubmission)
    => submission => (submission.Completed == completedSubmission) &&
                     !submission.Deleted &&
                     submission.EstablishmentId == establishmentId &&
                     submission.SectionId == sectionId;

    private static Expression<Func<Response, QuestionWithAnswer>> ToQuestionWithAnswer()
    => response => new QuestionWithAnswer()
    {
        QuestionRef = response.Question.ContentfulRef,
        QuestionText = response.Question.QuestionText ?? "", //Should this come from Contentful?
        AnswerRef = response.Answer.ContentfulRef,
        AnswerText = response.Answer.AnswerText ?? "",//Should this come from Contentful?
        DateCreated = response.DateCreated
    };

}
