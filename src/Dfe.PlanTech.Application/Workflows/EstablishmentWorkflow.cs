using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Repositories;

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
            var establishment = await _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentModel.Reference);
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
            var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync([establishmentReference]);
            return establishments.FirstOrDefault()?.Id;
        }

        public async Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences)
        {
            var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentReferences);
            return establishments.Select(e => e.AsDto());
        }

        public async Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
        {
            var links = await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(establishmentId);
            return links.Select(l => l.AsDto()).ToList();
        }

        public Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
        {
            return _storedProcedureRepository.RecordGroupSelection(userGroupSelectionModel);
        }

        public async Task<SqlGroupReadActivityDto?> GetLatestSelectedGroupSchool(int userId, int userEstablishmentId)
        {
            var latestSelectedGroupSchools = await _groupReadActivityRepository.GetGroupReadActivitiesAsync(userId, userEstablishmentId);
            return latestSelectedGroupSchools.FirstOrDefault()?.AsDto();
        }
    }
}
