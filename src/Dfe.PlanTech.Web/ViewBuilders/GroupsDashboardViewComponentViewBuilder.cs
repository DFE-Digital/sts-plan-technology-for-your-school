using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class GroupsDashboardViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    IEstablishmentService establishmentService,
    ISubmissionService submissionService,
    CurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser)
{
    private readonly IEstablishmentService _establishmentService = establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public Task<GroupsDashboardViewComponentViewModel> BuildViewModelAsync(QuestionnaireCategoryEntry category)
    {
        if (!category.Sections.Any())
        {
            Logger.LogError("No sections found for category {id}", category.Id);
            throw new InvalidDataException($"No sections found for category {category.Id}");
        }

        return GenerateViewModel(category);
    }

    private async Task<GroupsDashboardViewComponentViewModel> GenerateViewModel(QuestionnaireCategoryEntry category)
    {
        var userId = GetUserIdOrThrowException();
        var establishmentId = GetEstablishmentIdOrThrowException();
        var selectedSchool = await _establishmentService.GetLatestSelectedGroupSchoolAsync(userId, establishmentId);

        List<SqlSectionStatusDto> sectionStatuses = [];
        string? progressRetrievalErrorMessage = null;
        try
        {
            sectionStatuses = await _submissionService.GetSectionStatusesForSchoolAsync(establishmentId, category.Sections.Select(s => s.Id));
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An exception has occurred while trying to retrieve section progress with the following message: {message}",
                ex.Message
            );
            progressRetrievalErrorMessage = "Unable to retrieve progress, please refresh your browser.";
        }

        var description = category.Content is { Count: > 0 } content
            ? content[0]
            : new MissingComponentEntry();

        return new GroupsDashboardViewComponentViewModel
        {
            Description = description,
            GroupsCategorySections = await GetGroupsCategorySectionViewModel(category, sectionStatuses, progressRetrievalErrorMessage is not null).ToListAsync(),
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
        };
    }


    private async IAsyncEnumerable<GroupsCategorySectionViewModel> GetGroupsCategorySectionViewModel(
        QuestionnaireCategoryEntry category,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError
    )
    {
        List<GroupsCategorySectionViewModel> viewModels = [];
        foreach (var section in category.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                Logger.LogError("No slug found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId.Equals(section.Id));
            var recommendationIntro = await BuildCategorySectionRecommendationViewModel(section, sectionStatus);

            yield return new GroupsCategorySectionViewModel(section, recommendationIntro, sectionStatus, hadRetrievalError);
        }
    }
}
