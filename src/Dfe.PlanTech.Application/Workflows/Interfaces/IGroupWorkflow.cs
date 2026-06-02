using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IGroupWorkflow
{
    Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds);
    Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(int[] establishmentIds, string sectionId);
}
