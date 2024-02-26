using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetAllAnswersForSubmissionQuery(IPlanTechDbContext db) : IGetAllAnswersForLatestSubmissionQuery
{
    /// <summary>
    /// Returns all the answers for the latest completed submission for any given section and establishment.
    /// </summary>
    /// <param name="establishmentRef"></param>
    /// <returns></returns>
    public async Task<List<Answer>> GetAllAnswersForLatestSubmission(string section, int establishmentId)
    {
        var answers = await db.GetAnswersForLatestSubmissionBySectionId(section, establishmentId);

        return answers;
    }
}