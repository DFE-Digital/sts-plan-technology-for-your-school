using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.Models
{
    public class GroupEstablishmentModel
    {
        public IReadOnlyCollection<SqlEstablishmentDto> Establishments { get; init; } = [];
        public int[] EstablishmentIds { get; init; } = [];
        public IReadOnlyCollection<SqlSubmissionDto> CompletedSubmissions { get; init; } = [];
    }
}
