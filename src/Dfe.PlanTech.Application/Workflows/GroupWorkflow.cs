using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository) : IGroupWorkflow
{
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds);

        return submissions.Select(s => s.AsDto()).ToList();
    }
}
