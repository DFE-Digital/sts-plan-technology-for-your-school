using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class ResponseRepository
{
    protected readonly PlanTechDbContext _db;

    public ResponseRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public IQueryable<ResponseEntity> GetLatestResponsesBySectionIdQueryable(int establishmentId, string sectionId, bool completedSubmission)
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

}
