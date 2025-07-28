using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategorySectionViewComponentViewBuilder(
    ILoggerFactory loggerFactory,
    ContentfulService contentfulService,
    CurrentUser currentUser,
    SubmissionService submissionService
) : BaseViewBuilder(loggerFactory, contentfulService, currentUser)
{
    private readonly ILogger<CategorySectionViewComponent> _logger = loggerFactory.CreateLogger<CategorySectionViewComponent>();
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
            _logger.LogError(
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
            CategorySections = await BuildCategorySectionViewModel(category, sectionStatuses, progressRetrievalErrorMessage is null).ToListAsync(),
            CompletedSectionCount = sectionStatuses.Count(ss => ss.Completed),
            Description = description,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
            TotalSectionCount = category.Sections.Count
        };
    }

    private async IAsyncEnumerable<CategorySectionViewModel> BuildCategorySectionViewModel(
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

            yield return new CategorySectionViewModel(section, recommendationIntro, sectionStatus, hadRetrievalError);
        }
    }
}
