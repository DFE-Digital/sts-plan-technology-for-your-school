using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Utilities;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategoryLandingViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ISubmissionService submissionService,
    ICurrentUser currentUser
) : BaseViewBuilder(logger, contentfulService, currentUser), ICategoryLandingViewComponentViewBuilder
{
    private readonly ISubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    private const string CategoryLandingSections = "Components/CategoryLanding/Sections";
    private const string CategoryLandingSectionsPrintContent = "Components/CategoryLanding/SectionsPrintContent";

    private const string CategoryLandingSectionAssessmentLink = "Components/CategoryLanding/SectionAssessmentLink";
    private const string CategoryLandingSectionAssessmentLinkPrintContent = "Components/CategoryLanding/SectionAssessmentLinkPrintContent";

    public async Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(
        QuestionnaireCategoryEntry category,
        string slug,
        string? sectionName,
        string? sortOrder,
        bool print = false)
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

        var sortType = sortOrder.GetRecommendationSortEnumValue();
        var categoryLandingSections = await BuildCategoryLandingSectionViewModels(establishmentId, category, sectionStatuses, progressRetrievalErrorMessage is not null, sortType).ToListAsync();
        var completedSectionCount = sectionStatuses.Count(ss => ss.LastCompletionDate != null);

        var viewModel = new CategoryLandingViewComponentViewModel
        {
            AllSectionsCompleted = completedSectionCount.Equals(category.Sections.Count),
            AnySectionsCompleted = completedSectionCount > 0,
            CategoryLandingSections = categoryLandingSections,
            CategoryName = category.Header.Text,
            CategorySlug = slug,
            Sections = category.Sections,
            SectionName = sectionName,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
            SortType = sortType,
            Print = print,
            SectionsPartialName = print ? CategoryLandingSectionsPrintContent : CategoryLandingSections,
            StatusLinkPartialName = print ? CategoryLandingSectionAssessmentLinkPrintContent : CategoryLandingSectionAssessmentLink
        };

        return viewModel;
    }

    private async IAsyncEnumerable<CategoryLandingSectionViewModel> BuildCategoryLandingSectionViewModels(
        int establishmentId,
        QuestionnaireCategoryEntry category,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError,
        RecommendationSort sortType
    )
    {
        foreach (var section in category.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                Logger.LogError("No slug found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId.Equals(section.Id));
            var recommendations = await GetCategoryLandingSectionRecommendations(establishmentId, section, sortType);

            yield return new CategoryLandingSectionViewModel(
                section,
                recommendations,
                sectionStatus,
                hadRetrievalError
            );
        }
    }

    private async Task<CategoryLandingSectionRecommendationsViewModel> GetCategoryLandingSectionRecommendations(
        int establishmentId,
        QuestionnaireSectionEntry section,
        RecommendationSort sortType
    )
    {
        try
        {
            if (section.InterstitialPage is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find {section.Name} interstitial page");
            }

            var latestResponses = await _submissionService.GetLatestSubmissionResponsesModel(establishmentId, section, true)
                ?? throw new DatabaseException($"Could not find user's answers for section {section.Name}");

            var recommendationChunks = section.CoreRecommendations;
            var recommendationReferences = recommendationChunks.Select(r => r.Id);
            var recommendations = await _submissionService.GetLatestRecommendationStatusesByRecommendationIdAsync(recommendationReferences, establishmentId);
            var sortedRecommendations = recommendationChunks.SortByStatus(recommendations, sortType);
            var chunks = sortedRecommendations.Select(sr => new RecommendationChunkViewModel
            {
                HeaderText = sr.HeaderText,
                LastUpdated = recommendations[sr.Id].DateCreated,
                Status = RecommendationStatusHelper.GetStatus(sr, recommendations),
                SlugifiedLinkText = sr.SlugifiedLinkText
            })
                .ToList();

            return new CategoryLandingSectionRecommendationsViewModel
            {
                SectionName = section.Name,
                SectionSlug = section.InterstitialPage.Slug,
                Answers = latestResponses.Responses,
                Chunks = chunks
            };
        }
        catch
        {
            return new CategoryLandingSectionRecommendationsViewModel
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }
}
