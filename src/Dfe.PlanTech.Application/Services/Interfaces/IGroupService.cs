using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IGroupService
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(int[] establishmentIds);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(int dboGroupId, string sectionId);
    Task<Dictionary<string, int>> GetGroupCompletedSubmissionCountBySection(int dboGroupId);
    Task<Dictionary<string, int>> GetGroupCompletedSubmissionCountBySection(IEnumerable<int> dboSchoolIds);
    Task<Dictionary<string, int>> GetGroupCompletedSubmissionCountBySection(IEnumerable<string> urns);
    Task<GroupEstablishmentDTO?> GetGroupWithEstablishmentsFromGIASAndCreateInDbo(int groupEstId);
    Task<GroupEstablishmentDTO?> GetGroupHomePageModel(int groupEstId);
    Task<bool> IsSchoolWithinGroup(int groupId, string urn);
    Task<bool> IsSchoolWithinGroup(int dboGroupId, IEnumerable<string> urns);

    Task<bool> AreAllSchoolsWithinGroup(int groupId, IEnumerable<string> urns);
}
