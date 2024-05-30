using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Responses.Queries;

public class GetLatestResponsesQuery : IGetLatestResponsesQuery
{
    private readonly IPlanTechDbContext _db;

    public GetLatestResponsesQuery(IPlanTechDbContext db)
    {
        _db = db;
    }

    public async Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default)
    {
        var responseListByDate = GetCurrentSubmission(establishmentId, sectionId)
                                        .SelectMany(submission => submission.Responses)
                                        .Where(response => response.Question.ContentfulRef == questionId)
                                        .OrderByDescending(response => response.DateCreated)
                                        .Select(ToQuestionWithAnswer());

        return await _db.FirstOrDefaultAsync(responseListByDate, cancellationToken);
    }

    public async Task<CheckAnswerDto?> GetLatestResponses(int establishmentId, string sectionId, CancellationToken cancellationToken = default)
    {
        var latestCheckAnswerDto = await _db.FirstOrDefaultAsync(GetLatestResponsesBySectionIdQueryable(establishmentId, sectionId), cancellationToken);

        bool haveSubmission = latestCheckAnswerDto != null &&
                                    latestCheckAnswerDto.SubmissionId > 0 &&
                                    latestCheckAnswerDto.Responses != null;
        return haveSubmission ? latestCheckAnswerDto : null;
    }

    private IQueryable<CheckAnswerDto> GetLatestResponsesBySectionIdQueryable(int establishmentId, string sectionId)
    => GetCurrentSubmission(establishmentId, sectionId)
            .Select(submission => new CheckAnswerDto()
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

    private IQueryable<Submission> GetCurrentSubmission(int establishmentId, string sectionId)
    => _db.GetSubmissions
            .Where(IsMatchingIncompleteSubmission(establishmentId, sectionId))
            .OrderByDescending(submission => submission.DateCreated);

    private static Expression<Func<Submission, bool>> IsMatchingIncompleteSubmission(int establishmentId, string sectionId)
    => submission => !submission.Completed &&
                     !submission.Deleted &&
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