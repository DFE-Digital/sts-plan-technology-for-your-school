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
    IUserService userService,
    ICurrentUser currentUser
)
    : BaseViewBuilder(logger, contentfulService, currentUser),
        ICategoryLandingViewComponentViewBuilder
{
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IUserService _userService =
        userService ?? throw new ArgumentNullException(nameof(userService));

    private const string CategoryLandingSectionAssessmentLink =
        "Components/CategoryLanding/SectionAssessmentLink";
    private const string CategoryLandingSectionAssessmentLinkPrintContent =
        "Components/CategoryLanding/SectionAssessmentLinkPrintContent";

    public async Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(
        QuestionnaireCategoryEntry category,
        string slug,
        string? sectionName,
        string? sortOrder,
        bool print = false
    )
    {
        if (category.Sections.Count == 0)
        {
            Logger.LogError("Found no sections for category {Id}", category.Id);
            throw new InvalidDataException($"Found no sections for category {category.Id}");
        }

        var establishmentId = await GetActiveEstablishmentIdOrThrowException();

        List<SqlSectionStatusDto> sectionStatuses = [];
        string? progressRetrievalErrorMessage = null;
        try
        {
            sectionStatuses = await _submissionService.GetSectionStatusesForSchoolAsync(
                establishmentId,
                category.Sections.Select(s => s.Id)
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An exception has occurred while trying to retrieve section progress with the following message: {message}",
                ex.Message
            );
            progressRetrievalErrorMessage =
                "Unable to retrieve progress, please refresh your browser.";
        }

        var sortType = await GetUserSortType(sortOrder);

        var categoryLandingSections = await BuildCategoryLandingSectionViewModels(
                establishmentId,
                category,
                sectionStatuses,
                progressRetrievalErrorMessage is not null,
                sortType
            )
            .ToListAsync();
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
            StatusLinkPartialName = print
                ? CategoryLandingSectionAssessmentLinkPrintContent
                : CategoryLandingSectionAssessmentLink,
        };

        return viewModel;
    }

    private async IAsyncEnumerable<CategoryLandingSectionViewModel> BuildCategoryLandingSectionViewModels(
        int establishmentId,
        QuestionnaireCategoryEntry category,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError,
        RecommendationSortOrder sortType
    )
    {
        foreach (var section in category.Sections ?? [])
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                Logger.LogError(
                    "No slug found for subtopic with ID {SectionId} and name {SectionName}",
                    section.Id,
                    section.Name
                );
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus =>
                sectionStatus.SectionId.Equals(section.Id)
            );
            var recommendations = await GetCategoryLandingSectionRecommendations(
                establishmentId,
                section,
                sortType
            );

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
        RecommendationSortOrder sortType
    )
    {
        try
        {
            if (section.InterstitialPage is null)
            {
                throw new ContentfulDataUnavailableException(
                    $"Could not find {section.Name} interstitial page"
                );
            }

            var latestResponses =
                await _submissionService.GetLatestSubmissionResponsesModel(
                    establishmentId,
                    section,
                    status: SubmissionStatus.CompleteReviewed
                )
                ?? throw new DatabaseException(
                    $"Could not find user's answers for section {section.Name}"
                );

            var recommendationChunks = section.CoreRecommendations;
            var recommendations =
                await _submissionService.GetLatestRecommendationStatusesByEstablishmentIdAsync(
                    establishmentId
                );
            var sortedRecommendations = recommendationChunks.SortByStatus(
                recommendations,
                sortType
            );
            var chunks = sortedRecommendations
                .Select(sr => new RecommendationChunkViewModel
                {
                    Header = sr.HeaderText,
                    LastUpdated = recommendations[sr.Id].DateCreated,
                    Status = RecommendationStatusHelper.GetStatus(sr, recommendations),
                    Slug = sr.Slug,
                })
                .ToList();

            return new CategoryLandingSectionRecommendationsViewModel
            {
                SectionName = section.Name,
                SectionSlug = section.InterstitialPage.Slug,
                Answers = latestResponses.Responses,
                Chunks = chunks,
            };
        }
        catch
        {
            return new CategoryLandingSectionRecommendationsViewModel
            {
                NoRecommendationFoundErrorMessage =
                    $"Unable to retrieve {section.Name} recommendation",
            };
        }
    }

    private async Task<RecommendationSortOrder> GetUserSortType(string? sortOrder)
    {
        var sortType = sortOrder?.GetRecommendationSortEnumValue();
        if (CurrentUser.UserId != null)
        {
            if (sortType != null)
            {
                await _userService.UpsertUserSettingsAsync(
                    CurrentUser.UserId.Value,
                    sortType.Value
                );
            }
            else
            {
                var userSettings = await _userService.GetUserSettingsByUserIdAsync(
                    CurrentUser.UserId.Value
                );
                sortType = userSettings?.SortOrder;
            }
        }

        return sortType ?? RecommendationSortOrder.Default;
    }
}
