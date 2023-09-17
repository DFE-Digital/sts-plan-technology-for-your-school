using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Responses.Interface
{
    public interface ICreateResponseCommand
    {
        Task<int> CreateResponse(RecordResponseDto recordResponseDto);
    }
}
