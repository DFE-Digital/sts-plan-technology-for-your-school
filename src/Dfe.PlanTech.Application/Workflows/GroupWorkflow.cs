using Contentful.Core.Models.Management;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows;

public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentService establishmentService,
    IEstablishmentRepository establishmentRepository) : IGroupWorkflow
{
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IEstablishmentRepository _establishmentRepository =
    establishmentRepository ?? throw new ArgumentNullException(nameof(establishmentRepository));

    public async Task<List<SqlSubmissionDto>> GetGroupSubmissionsBySections(int[] establishmentIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed)
    {
        var submissions = await _submissionRepository.GetLatestEstablishmentsSubmissionsByEstablishmentAndSectionAsync(establishmentIds, status);

        return submissions.Select(s => s.AsDto()).ToList();
    }

    public async Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromUrns(IEnumerable<string> urns, SubmissionStatus status = SubmissionStatus.CompleteReviewed)
    {
        return await _submissionRepository.GetSubmissionsCountBySectionAsync(urns, status);
    }
    public async Task<Dictionary<string, int>> GetGroupSubmissionsCountBySectionsFromIds(IEnumerable<int> dboSchoolIds, SubmissionStatus status = SubmissionStatus.CompleteReviewed)
    {
        return await _submissionRepository.GetSubmissionsCountBySectionAsync(dboSchoolIds, status);
    }

    public async Task<List<SubmissionInformationModel>> GetGroupSubmissionInformationForSection(IEnumerable<EstablishmentBasicDto> establishments, string sectionId)
    {
        var dboSchoolIds = establishments.Select(e => e?.DboId ?? 0).ToList() ?? [];
        var groupLatestSubmissions = await _submissionRepository.GetLatestSubmissionPerEstablishmentForSectionAsync(dboSchoolIds, sectionId)
            ?? [];

        var groupSubmissionInfo = new List<SubmissionInformationModel>();

        foreach (var est in establishments)
        {
            var submission = groupLatestSubmissions
                    .FirstOrDefault(s => s.Establishment.EstablishmentRef == est.Urn);

            if (submission == null)
            {               
                groupSubmissionInfo.Add(
                    new SubmissionInformationModel
                    {
                        EstablishmentId = est.DboId.Value,
                        EstablishmentName = est.Name,
                        EstablishmentRef = est.Urn,
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

    public async Task <EstablishmentEntity?> GetGroupFromDboEstablishmentAsync(int id)
    {
        return await _establishmentRepository.GetEstablishmentByIdAsync(id);
    }
}
