using Dfe.PlanTech.Domain.Answers.Models;

namespace Dfe.PlanTech.Domain.Users.Interfaces;

public interface IGetAllAnswersForLatestSubmissionQuery
{ 
    Task<List<Answer>> GetAllAnswersForLatestSubmission(string section, int establishmentId);
}