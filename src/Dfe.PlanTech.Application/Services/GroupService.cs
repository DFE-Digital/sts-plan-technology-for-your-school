using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class GroupService(IGroupWorkflow groupWorkflow) : IGroupService
{
    private readonly IGroupWorkflow _groupWorkflow =
        groupWorkflow ?? throw new ArgumentNullException(nameof(groupWorkflow));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissionsBySections(int[] establishmentIds)
    {
        var submissions = await _groupWorkflow.GetGroupCompletedSubmissions(establishmentIds);
        return submissions;
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(int[] establishmentIds, string sectionId)
    {
        var submissions = await _groupWorkflow.GetGroupSubmissionInformationForSection(establishmentIds, sectionId);
        return submissions;
    }
}
