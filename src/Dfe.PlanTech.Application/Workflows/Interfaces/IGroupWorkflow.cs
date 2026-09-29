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
    Task<List<SqlSubmissionDto>> GetGroupSubmissionsBySections(IEnumerable<int> establishmentIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(IEnumerable<EstablishmentBasicDto> ests, string sectionId);
    Task<Dictionary<string, int>> GetGroupSubmissionsCountBySections(int dboGroupId, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null);
    Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromIds(IEnumerable<int> dboSchoolIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed, string? sectionId = null);
    Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsFromGIASAndCreateInDbo(int dboGroupId);
    Task<GroupEstablishmentDTO?> GetGroupHomePageModel(int groupEstId);
    Task<bool> IsSchoolWithinGroup(int groupId, string urn);
    Task<bool> IsSchoolWithinGroup(int dboGroupId, IEnumerable<string> urns);

    Task<bool> AreAllSchoolsWithinGroup(int groupId, IEnumerable<string> urns);
}
