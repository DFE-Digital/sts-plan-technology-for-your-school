using Contentful.Core.Models.Management;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using System.Collections;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGroupWorkflow
{
    Task<List<SqlSubmissionDto>> GetGroupSubmissionsBySections(int[] establishmentIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(IEnumerable<EstablishmentBasicDto> ests, string sectionId);
    Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromUrns(IEnumerable<string> urns, SubmissionStatus status = SubmissionStatus.CompleteReviewed);
    Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromIds(IEnumerable<int> dboSchoolIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed);
    Task<EstablishmentEntity?> GetGroupFromDboEstablishmentAsync(int id);
}
