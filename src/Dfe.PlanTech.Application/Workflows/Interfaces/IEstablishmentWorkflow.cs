﻿using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IEstablishmentWorkflow
    {
        Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference);
        Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences);
        Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId);
        Task<SqlGroupReadActivityDto?> GetLatestSelectedGroupSchool(int userId, int userEstablishmentId);
        Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(EstablishmentModel establishmentModel);
        Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName);
        Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel);
    }
}