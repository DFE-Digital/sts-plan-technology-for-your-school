using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetAllAnswersForLatestSubmissionQuery(IPlanTechDbContext db) : IGetAllAnswersForLatestSubmissionQuery
{
    /// <summary>
    /// Returns all the answers for the latest completed submission for any given section and establishment.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="establishmentId"></param>
    /// <returns></returns>
    public async Task<List<Answer>?> GetAllAnswersForLatestSubmission(string sectionId, int establishmentId)
    {
        var latestSubmissionIdQuery = db.GetSubmissions
            .Where(s => s.SectionId == sectionId && s.EstablishmentId == establishmentId && s.Completed && !s.Deleted)
            .OrderByDescending(s => s.DateCreated)
            .Select(s => s.Id);

        var latestSubmissionId = await db.FirstOrDefaultAsync(latestSubmissionIdQuery);

        if (latestSubmissionId == 0)
        {
            return null;
        }

        var answersQuery = db.GetSubmissions
            .Where(s => s.Id == latestSubmissionId)
            .SelectMany(submission => submission.Responses)
            .Join(
                db.GetAnswers,
                response => response.AnswerId,
                answer => answer.Id,
                (response, answer) => answer);

        var answers = await db.ToListAsync(answersQuery);

        return answers;
    }
}