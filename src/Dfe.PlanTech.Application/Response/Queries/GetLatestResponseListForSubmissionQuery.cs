using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Queries
{
    public class GetLatestResponseListForSubmissionQuery : IGetLatestResponseListForSubmissionQuery
    {
        private readonly IPlanTechDbContext _db;

        public GetLatestResponseListForSubmissionQuery(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<List<QuestionWithAnswer>> GetLatestResponseListForSubmissionBy(int submissionId)
        {
            var responseListByDate = _db.GetResponses
                            .Where(response => response.SubmissionId == submissionId)
                            .Select(response => new QuestionWithAnswer()
                            {
                                QuestionRef = response.Question.ContentfulRef,
                                QuestionText = response.Question.QuestionText ?? "",
                                AnswerRef = response.Answer.ContentfulRef,
                                AnswerText = response.Answer.AnswerText ?? "",
                                DateCreated = response.DateCreated
                            })
                            .GroupBy(questionWithAnswer => questionWithAnswer.QuestionRef)
                            .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First());

            return await _db.ToListAsync(responseListByDate);
        }
    }
}