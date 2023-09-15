using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Queries
{
    //TODO: Rename to "GetLatestResponseQuery", or similiar
    public class GetLatestResponseListForSubmissionQuery : IGetLatestResponseListForSubmissionQuery
    {
        private readonly IPlanTechDbContext _db;

        public GetLatestResponseListForSubmissionQuery(IPlanTechDbContext db)
        {
            _db = db;
        }

        //TODO: DELETE
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

        public async Task<List<QuestionWithAnswer>> GetResponseListByDateCreated(int submissionId)
        {
            var responseListByDate = _db.GetResponses
                            .Where(response => response.SubmissionId == submissionId)
                            .Select(response => new QuestionWithAnswer()
                            {
                                QuestionRef = response.Question.ContentfulRef,
                                AnswerRef = response.Answer.ContentfulRef,
                                DateCreated = response.DateCreated
                            })
                            .OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated);

            return await _db.ToListAsync(responseListByDate);
        }

        public Task<QuestionWithAnswer?> GetLatestResponse(int establishmentId, string sectionId)
        {
            var responseListByDate = _db.GetResponses
                            .Where(IsMatchingIncompleteSubmission(establishmentId, sectionId))
                            .Select(response => new QuestionWithAnswer()
                            {
                                QuestionRef = response.Question.ContentfulRef,
                                AnswerRef = response.Answer.ContentfulRef,
                                DateCreated = response.DateCreated
                            })
                            .OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated);

            return _db.FirstOrDefaultAsync(responseListByDate);
        }

        private static Expression<Func<Domain.Responses.Models.Response, bool>> IsMatchingIncompleteSubmission(int establishmentId, string sectionId)
        => response => response.Submission.Completed == false &&
                        response.Submission.EstablishmentId == establishmentId &&
                        response.Submission.SectionId == sectionId;
    }
}