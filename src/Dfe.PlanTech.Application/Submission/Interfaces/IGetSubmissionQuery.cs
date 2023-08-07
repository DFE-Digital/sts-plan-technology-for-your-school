namespace Dfe.PlanTech.Application.Submission.Interface;

using Dfe.PlanTech.Domain.Submissions.Models;

public interface IGetSubmissionQuery
{
    Task<Submission?> GetSubmissionBy(int establishmentId, string sectionId);
}