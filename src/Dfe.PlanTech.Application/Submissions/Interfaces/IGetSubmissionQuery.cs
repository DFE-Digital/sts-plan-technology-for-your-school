namespace Dfe.PlanTech.Application.Submissions.Interface;

using Dfe.PlanTech.Domain.Submissions.Models;

public interface IGetSubmissionQuery
{
    Task<Submission?> GetSubmissionBy(int establishmentId, string sectionId);
}