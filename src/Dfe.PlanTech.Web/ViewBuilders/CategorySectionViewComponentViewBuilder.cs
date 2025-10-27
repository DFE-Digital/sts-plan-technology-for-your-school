using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategorySectionViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), ICategorySectionViewComponentViewBuilder
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public async Task<CategorySectionViewComponentViewModel> BuildViewModelAsync(QuestionnaireCategoryEntry category)
    {
        if (!category.Sections.Any())
        {
            Logger.LogError("Found no sections for category {id}", category.Id);
            throw new InvalidDataException($"Found no sections for category {category.Id}");
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();

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

        var categoryLandingSlug = GetLandingPageSlug(category);
        var categorySections = await BuildCategorySectionViewModel(category.Sections, sectionStatuses, progressRetrievalErrorMessage is null).ToListAsync();
        var description = category.Content is { Count: > 0 } content
            ? content[0]
            : new MissingComponentEntry();

        return new CategorySectionViewComponentViewModel
        {
            CategoryHeaderText = category.Header.Text,
            CategorySections = categorySections,
            CategorySlug = categoryLandingSlug,
            CompletedSectionCount = sectionStatuses.Count(ss => ss.Completed),
            Description = description,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
            TotalSectionCount = category.Sections.Count
        };
    }

    private async IAsyncEnumerable<CategorySectionViewModel> BuildCategorySectionViewModel(
        ICollection<QuestionnaireSectionEntry> sections,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError
    )
    {
        foreach (var section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                Logger.LogError("No slug found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId.Equals(section.Id));
            var recommendationIntro = BuildCategorySectionRecommendationViewModel(section, sectionStatus);

            yield return new CategorySectionViewModel(section, recommendationIntro, sectionStatus, hadRetrievalError);
        }
    }

    private string? GetLandingPageSlug(QuestionnaireCategoryEntry category)
    {
        if (category?.LandingPage?.Slug is string slug)
        {
            return slug;
        }

        Logger.LogError("Could not find category landing slug for category {CategoryInternalName}", category?.InternalName ?? "unknown category");
        return null;
    }
}
