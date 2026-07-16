using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentService establishmentService) : IGroupWorkflow
{
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));

    public async Task<List<SqlSubmissionDto>> GetGroupCompletedSubmissions(int[] establishmentIds)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(establishmentIds);

        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(List<SqlEstablishmentLinkDto> establishmentLinks, string sectionId)
    {
        var establishments = new List<SqlEstablishmentDto>();

        foreach (var e in establishmentLinks)
        {
            establishments.Add(
                await _establishmentService.GetOrCreateEstablishmentAsync(
                    e.Urn,
                    e.EstablishmentName));
        }

        var establishmentIds = establishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(establishmentIds, sectionId)
            ?? [];

        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        foreach (var est in establishments)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.Establishment.EstablishmentRef == est.EstablishmentRef);

            if (submission == null)
            {               
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = est.Id,
                        EstablishmentName = est.OrgName ?? "",
                        EstablishmentRef = est.EstablishmentRef ?? "",
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
                        EstablishmentId = submission.EstablishmentId,
                        EstablishmentName = est.OrgName ?? "",
                        EstablishmentRef = est.EstablishmentRef ?? "",
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
