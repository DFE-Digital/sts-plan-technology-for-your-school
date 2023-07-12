using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Response.Interface
{
    public interface ICreateResponseCommand
    {
        Task<int> CreateResponse(RecordResponseDto recordResponseDto);
    }
}
