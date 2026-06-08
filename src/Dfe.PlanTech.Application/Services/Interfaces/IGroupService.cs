using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IGroupService
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(int[] establishmentIds);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(string[] establishmentRefs, string sectionId);
}
