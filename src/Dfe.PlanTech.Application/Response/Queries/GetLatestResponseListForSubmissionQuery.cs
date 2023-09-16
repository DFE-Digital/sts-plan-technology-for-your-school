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
                            .Select(ToQuestionWithAnswer())
                            .OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated);

            return await _db.ToListAsync(responseListByDate);
        }

        private static Expression<Func<Domain.Responses.Models.Response, QuestionWithAnswer>> ToQuestionWithAnswer()
        => response => new QuestionWithAnswer()
        {
            QuestionRef = response.Question.ContentfulRef,
            QuestionText = response.Question.QuestionText, //Should this come from Contentful?
            AnswerRef = response.Answer.ContentfulRef,
            AnswerText = response.Answer.AnswerText,//Should this come from Contentful?
            DateCreated = response.DateCreated
        };

        public Task<QuestionWithAnswer?> GetLatestResponse(int establishmentId, string sectionId)
        {
            var responseListByDate = GetQuestionsWithAnswers(establishmentId, sectionId).Select(qwa => qwa.FirstOrDefault());

            return _db.FirstOrDefaultAsync(responseListByDate);
        }

        public Task<IEnumerable<QuestionWithAnswer>?> GetResponses(int establishmentId, string sectionId)
            => _db.FirstOrDefaultAsync(GetQuestionsWithAnswers(establishmentId, sectionId));

        private IQueryable<IEnumerable<QuestionWithAnswer>> GetQuestionsWithAnswers(int establishmentId, string sectionId)
        => GetCurrentSubmission(establishmentId, sectionId)
                .Select(submission => submission.Responses
                    .Select(response => new QuestionWithAnswer
                    {
                        QuestionRef = response.Question.ContentfulRef,
                        AnswerRef = response.Answer.ContentfulRef,
                        DateCreated = response.DateCreated
                    })
                    .GroupBy(questionWithAnswer => questionWithAnswer.QuestionRef)
                    .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First()));

        private IQueryable<Domain.Submissions.Models.Submission> GetCurrentSubmission(int establishmentId, string sectionId)
        => _db.GetSubmissions
                .Where(IsMatchingIncompleteSubmission(establishmentId, sectionId))
                .OrderByDescending(submission => submission.DateCreated);

        private static Expression<Func<Domain.Submissions.Models.Submission, bool>> IsMatchingIncompleteSubmission(int establishmentId, string sectionId)
        => submission => submission.Completed == false &&
                        submission.EstablishmentId == establishmentId &&
                        submission.SectionId == sectionId;
    }
}
