using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Responses.Interface
{
    public interface ICreateResponseCommand
    {
        //TODO: DELETE
        Task<int> CreateResponse(RecordResponseDto recordResponseDto);

        Task<int> CreateResponsNew(RecordResponseDto recordResponseDto);
    }
}
