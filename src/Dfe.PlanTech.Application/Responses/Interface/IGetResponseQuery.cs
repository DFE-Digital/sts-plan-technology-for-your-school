namespace Dfe.PlanTech.Application.Responses.Interface;

using Dfe.PlanTech.Domain.Responses.Models;

public interface IGetResponseQuery
{
    Task<Response[]?> GetResponseListBy(int submissionId);
}