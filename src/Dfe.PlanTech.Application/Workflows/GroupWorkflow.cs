using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
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

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(int[] establishmentIds, string sectionId)
    {
        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId);

        foreach (var id in establishmentIds)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.EstablishmentId == id);

            if (submission == null)
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = id,
                        SectionId = sectionId,
                        Status = SubmissionStatus.NotStarted
                    }
                );
            }
            else
            {
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = id,
                        SectionId = sectionId,
                        SubmissionId = submission.Id,
                        DateCreated = submission.DateCreated,
                        DateLastUpdated = submission.DateLastUpdated,
                        DateCompleted = submission.DateCompleted,
                        Status = submission.Status
                    }
                );
            }
        }

        return groupSubmissionInfo;
    }
}
