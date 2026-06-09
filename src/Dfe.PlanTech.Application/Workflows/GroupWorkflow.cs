using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentRepository establishmentRepository) : IGroupWorkflow
{
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IEstablishmentRepository _establishmentRepository =
        establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds);

        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(string[] establishmentRefs, string sectionId)
    {
        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentRefs) ?? [];

        var establishmentIds = establishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            ?? [];

        foreach (var estRef in establishmentRefs)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.Establishment.EstablishmentRef == estRef);

            if (submission == null)
            {
                var school = await _establishmentRepository.GetEstablishmentByReferenceAsync(estRef);

                if (school == null || school.OrgName == null || school.EstablishmentRef == null)
                {
                    throw new InvalidDataException($"No establishment found with ref: {estRef}");
                }

                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = school.Id,
                        EstablishmentName = school.OrgName,
                        EstablishmentRef = school.EstablishmentRef,
                        SectionId = sectionId,
                        Status = SubmissionStatus.NotStarted
                    }
                );
            }
            else
            {
                var establishmentName = submission.Establishment.OrgName ?? throw new InvalidDataException($"No details found for establishment reference: {estRef}");

                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = submission.EstablishmentId,
                        EstablishmentName = establishmentName,
                        EstablishmentRef = estRef,
                        SectionId = sectionId,
                        SubmissionId = submission.Id,
                        DateCreated = DateTimeHelper.FormattedDateShort(submission.DateCreated),
                        DateLastUpdated = submission.DateLastUpdated != null
                            ? DateTimeHelper.FormattedDateShort(submission.DateLastUpdated.Value)
                            : null,
                        DateCompleted = submission.DateCompleted != null
                            ? DateTimeHelper.FormattedDateShort(submission.DateCompleted.Value)
                            : null,
                        Status = submission.Status
                    }
                );
            }
        }

        return groupSubmissionInfo;
    }
}
