namespace Dfe.PlanTech.Application.Response.Interface;

using Dfe.PlanTech.Domain.Responses.Models;

public interface IGetResponseQuery
{
    Task<Response[]?> GetResponseListBy(int submissionId);
}