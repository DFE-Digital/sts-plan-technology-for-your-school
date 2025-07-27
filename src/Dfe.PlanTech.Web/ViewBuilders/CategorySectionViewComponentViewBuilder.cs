using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategorySectionViewComponentViewBuilder(
    ILogger<CategorySectionViewComponent> logger,
    CurrentUser currentUser,
    ContentfulService contentfulService,
    SubmissionService submissionService
) : BaseViewBuilder(currentUser)
{
    private readonly ILogger<CategorySectionViewComponent> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public async Task<CategorySectionViewComponentViewModel> BuildViewModelAsync(CmsCategoryDto category)
    {
        if (!category.Sections.Any())
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);
            throw new InvalidDataException($"Found no sections for category {category.Sys.Id}");
        }

        var establishmentId = GetEstablishmentIdOrThrowException();

        List<SqlSectionStatusDto> sectionStatuses = [];
        string? progressRetrievalErrorMessage = null;
        try
        {
            sectionStatuses = await _submissionService.GetSectionStatusesForSchoolAsync(category, establishmentId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An exception has occurred while trying to retrieve section progress with the following message: {message}",
                ex.Message
            );
            progressRetrievalErrorMessage = "Unable to retrieve progress, please refresh your browser.";
        }

        var description = category.Content is { Count: > 0 } content
            ? content[0]
            : new CmsMissingComponentDto();

        return new CategorySectionViewComponentViewModel
        {
            CategorySections = BuildCategorySectionViewModel(category, sectionStatuses, progressRetrievalErrorMessage is null).ToList(),
            CompletedSectionCount = sectionStatuses.Count(ss => ss.Completed),
            Description = description,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
            TotalSectionCount = category.Sections.Count
        };
    }

    private async Task<IEnumerable<CategorySectionViewModel>> BuildCategorySectionViewModel(
        CmsCategoryDto category,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError
    )
    {
        List<CategorySectionViewModel> viewModels = [];
        foreach (var section in category.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                _logger.LogError("No slug found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId.Equals(section.Id));
            var recommendationIntro = await BuildCategorySectionRecommendationViewModel(section, sectionStatus);

            viewModels.Add(new CategorySectionViewModel(section, recommendationIntro, sectionStatus, hadRetrievalError));
        }

        return viewModels;
    }

    private async Task<CategorySectionRecommendationViewModel> BuildCategorySectionRecommendationViewModel(
        CmsQuestionnaireSectionDto section,
        SqlSectionStatusDto? sectionStatus
    )
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
        {
            return new CategorySectionRecommendationViewModel();
        }

        try
        {
            var recommendationIntro = await _contentfulService.GetSubtopicRecommendationIntroAsync(section.Id, sectionStatus.LastMaturity);
            if (recommendationIntro == null)
            {
                return new CategorySectionRecommendationViewModel
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }

            return new CategorySectionRecommendationViewModel
            {
                RecommendationSlug = recommendationIntro.Slug,
                RecommendationDisplayName = recommendationIntro.Header.Text,
                SectionSlug = section.InterstitialPage?.Slug,
                Viewed = sectionStatus.HasBeenViewed
            };
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An exception has occurred while trying to retrieve the recommendation for Section {sectionName}, with the message {errMessage}",
                section.Name,
                e.Message
            );
            return new CategorySectionRecommendationViewModel
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }
}
