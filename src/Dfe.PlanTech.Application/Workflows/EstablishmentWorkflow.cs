using System.Threading;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows
{
    public class EstablishmentWorkflow(
        EstablishmentRepository establishmentRepository,
        EstablishmentLinkRepository establishmentLinkRepository,
        GroupReadActivityRepository groupReadActivityRepository,
        StoredProcedureRepository storedProcedureRepository
    )
    {
        private readonly EstablishmentRepository _establishmentRepository = establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));
        private readonly EstablishmentLinkRepository _establishmentLinkRepository = establishmentLinkRepository ?? throw new ArgumentNullException(nameof(establishmentLinkRepository));
        private readonly GroupReadActivityRepository _groupReadActivityRepository = groupReadActivityRepository ?? throw new ArgumentNullException( nameof(groupReadActivityRepository));
        private readonly StoredProcedureRepository _storedProcedureRepository = storedProcedureRepository ?? throw new ArgumentNullException(nameof(storedProcedureRepository));

        public async Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel)
        {
            var establishment = await _establishmentRepository.GetEstablishmentByRefAsync(establishmentModel.Reference);
            establishment ??= await _establishmentRepository.CreateEstablishmentFromModelAsync(establishmentModel);

            return establishment.AsDto();
        }

        public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName)
        {
            var establishmentModel = new EstablishmentModel()
            {
                OrgName = establishmentName,
                Urn = establishmentUrn
            };

            return GetOrCreateEstablishmentAsync(establishmentModel);
        }

        public async Task<int?> GetEstablishmentIdByReferenceAsync(string establishmentReference)
        {
            var establishment = await _establishmentRepository.GetEstablishmentByRefAsync(establishmentReference);
            return establishment?.Id;
        }

        public async Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
        {
            var links = await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(establishmentId);
            return links.Select(l => l.AsDto()).ToList();
        }

        public Task<int> RecordGroupSelection(
            int userEstablishmentId,
            int selectedEstablishmentId,
            string? selectedEstablishmentName,
            int userId
        )
        {
            return _storedProcedureRepository.RecordGroupSelection(userEstablishmentId, selectedEstablishmentId, selectedEstablishmentName, userId);
        }

        public async Task<SqlGroupReadActivityDto?> GetLatestSelectedGroupSchool(int userId, int userEstablishmentId)
        {
            var latestSelectedGroupSchools = await _groupReadActivityRepository.GetGroupReadActivitiesAsync(userId, userEstablishmentId);
            return latestSelectedGroupSchools.FirstOrDefault()?.AsDto();
        }
    }
}
