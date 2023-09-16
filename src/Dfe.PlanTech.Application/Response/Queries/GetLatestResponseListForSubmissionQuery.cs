using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Queries
{
    //TODO: Rename to "GetLatestResponseQuery", or similiar
    //TODO: Refactor all queries to one or two - lot of duplication
    public class GetLatestResponseListForSubmissionQuery : IGetLatestResponseListForSubmissionQuery
    {
        private readonly IPlanTechDbContext _db;

        public GetLatestResponseListForSubmissionQuery(IPlanTechDbContext db)
        {
            _db = db;
        }

        public Task<QuestionWithAnswer?> GetLatestResponse(int establishmentId, string sectionId, CancellationToken cancellationToken = default)
        {
            var responseListByDate = GetLatestResponsesQueryable(establishmentId, sectionId).Select(qwa => qwa.FirstOrDefault());

            return _db.FirstOrDefaultAsync(responseListByDate, cancellationToken);
        }

        public Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default)
        {
            var responseListByDate = GetCurrentSubmission(establishmentId, sectionId)
                                            .SelectMany(submission => submission.Responses)
                                            .Where(response => response.Question.ContentfulRef == questionId)
                                            .OrderByDescending(response => response.DateCreated)
                                            .Select(ToQuestionWithAnswer());

            return _db.FirstOrDefaultAsync(responseListByDate, cancellationToken);
        }

        public Task<SubmissionWithResponses> GetLatestResponses(int establishmentId, string sectionId, CancellationToken cancellationToken = default)
            => _db.FirstOrDefaultAsync(GetLatestResponsesBySectionIdQueryable(establishmentId, sectionId), cancellationToken);

        private IQueryable<IEnumerable<QuestionWithAnswer>> GetLatestResponsesQueryable(int establishmentId, string sectionId)
        => GetCurrentSubmission(establishmentId, sectionId)
                .Select(submission => submission.Responses
                                                .Select(response => new QuestionWithAnswer
                                                {
                                                    QuestionRef = response.Question.ContentfulRef,
                                                    QuestionText = response.Question.QuestionText ?? "", //Should this come from Contentful?
                                                    AnswerRef = response.Answer.ContentfulRef,
                                                    AnswerText = response.Answer.AnswerText ?? "",//Should this come from Contentful?
                                                    DateCreated = response.DateCreated
                                                })
                                                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionRef)
                                                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First()));

        private IQueryable<SubmissionWithResponses> GetLatestResponsesBySectionIdQueryable(int establishmentId, string sectionId)
        => GetCurrentSubmission(establishmentId, sectionId)
                .Select(submission => new SubmissionWithResponses()
                {
                    Id = submission.Id,
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

        private IQueryable<Domain.Submissions.Models.Submission> GetCurrentSubmission(int establishmentId, string sectionId)
        => _db.GetSubmissions
                .Where(IsMatchingIncompleteSubmission(establishmentId, sectionId))
                .OrderByDescending(submission => submission.DateCreated);

        private static Expression<Func<Domain.Submissions.Models.Submission, bool>> IsMatchingIncompleteSubmission(int establishmentId, string sectionId)
        => submission => submission.Completed == false &&
                        submission.EstablishmentId == establishmentId &&
                        submission.SectionId == sectionId;

        private static Expression<Func<Domain.Responses.Models.Response, QuestionWithAnswer>> ToQuestionWithAnswer()
        => response => new QuestionWithAnswer()
        {
            QuestionRef = response.Question.ContentfulRef,
            QuestionText = response.Question.QuestionText ?? "", //Should this come from Contentful?
            AnswerRef = response.Answer.ContentfulRef,
            AnswerText = response.Answer.AnswerText ?? "",//Should this come from Contentful?
            DateCreated = response.DateCreated
        };

    }
}

public readonly record struct SubmissionWithResponses
{
    public SubmissionWithResponses()
    {
    }

    public int Id { get; init; }

    public List<QuestionWithAnswer> Responses { get; init; } = new();
}