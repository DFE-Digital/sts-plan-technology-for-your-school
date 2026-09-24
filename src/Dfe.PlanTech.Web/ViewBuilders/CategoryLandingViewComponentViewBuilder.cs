using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategoryLandingViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    ISubmissionService submissionService,
    IUserActionTrackingService userActionTrackingService,
    IEstablishmentService establishmentService,
    IUserService userService,
    IGroupService groupService
)
    : BaseViewBuilder(logger, contentfulService, currentUser),
        ICategoryLandingViewComponentViewBuilder
{
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IUserService _userService =
        userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly IUserActionTrackingService _userActionTrackingService =
        userActionTrackingService
        ?? throw new ArgumentNullException(nameof(userActionTrackingService));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));

    private const string CategoryLandingSectionAssessmentLink =
        "Components/CategoryLanding/SectionAssessmentLink";
    private const string CategoryLandingSectionAssessmentLinkPrintContent =
        "Components/CategoryLanding/SectionAssessmentLinkPrintContent";

    public async Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(
        QuestionnaireCategoryEntry category,
        string slug,
        string? sectionName,
        string? sortOrder,
        bool print = false,
        CategoryLandingContext context = CategoryLandingContext.School
    )
    {
        if (category.Sections.Count == 0)
        {
            Logger.LogError("Found no sections for category {Id}", category.Id);
            throw new InvalidDataException($"Found no sections for category {category.Id}");
        }

        return context switch
        {
            CategoryLandingContext.MAT => await BuildMatViewModelAsync(
                category,
                slug,
                sectionName
            ),

            _ => await BuildSchoolViewModelAsync(
                category,
                slug,
                sectionName,
                sortOrder,
                print
            ),
        };
    }

    private async Task<CategoryLandingViewComponentViewModel> BuildSchoolViewModelAsync(
        QuestionnaireCategoryEntry category,
        string slug,
        string? sectionName,
        string? sortOrder,
        bool print
    )
    {
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

        return new CategoryLandingViewComponentViewModel
        {
            CompletedSectionsCount = completedSectionCount,
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
            Context = CategoryLandingContext.School,
        };
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

            var userActionId = sectionStatus switch
            {
                {
                    Status: SubmissionStatus.InProgress or SubmissionStatus.CompleteNotReviewed,
                    LastUpdatedUserActionId: { } lastUpdatedUserActionId
                } => lastUpdatedUserActionId,

                {
                    Status: SubmissionStatus.CompleteReviewed,
                    CompletedUserActionId: { } completedUserActionId
                } => completedUserActionId,

                _ => (Guid?)null
            };

            string? establishmentName = null;

            if (userActionId is { } id)
            {
                var userAction = await _userActionTrackingService.GetAsync(id);

                if (
                    (userAction?.MatEstablishmentId ?? userAction?.EstablishmentId)
                    is { } userActionEstablishmentId
                )
                {
                    var userActionEstablishment =
                        await _establishmentService.GetEstablishmentByIdAsync(
                            userActionEstablishmentId
                        );
                    establishmentName = userActionEstablishment.OrgName;
                }
            }

            var recommendations =
                sectionStatus?.Status == SubmissionStatus.CompleteReviewed
                    ? await GetCategoryLandingSectionRecommendations(
                        establishmentId,
                        section,
                        sortType
                    )
                    : null;

            yield return new CategoryLandingSectionViewModel(
                section,
                recommendations,
                sectionStatus,
                hadRetrievalError,
                establishmentName
            );
        }
    }

    private async Task<CategoryLandingViewComponentViewModel> BuildMatViewModelAsync(
    QuestionnaireCategoryEntry category,
    string slug,
    string? sectionName
    )
    {
        try
        {
            var matEstablishmentId = GetUserOrganisationIdOrThrowException();

            var establishmentLinks =
                await _establishmentService.GetEstablishmentLinks(matEstablishmentId) ?? [];

            var establishmentUrns = establishmentLinks
                .Select(e => e.Urn)
                .Where(urn => !string.IsNullOrWhiteSpace(urn))
                .Distinct()
                .ToArray();

            var establishments =
                await _establishmentService.GetEstablishmentsByReferencesAsync(establishmentUrns)
                ?? [];

            var establishmentIds = establishments
                .Select(e => e.Id)
                .Distinct()
                .ToArray();

            var completedSubmissions =
                establishmentIds.Length != 0
                    ? await _groupService.GetGroupCompletedSubmissionsBySections(establishmentIds) ?? []
                    : [];

            var completedCountBySectionId = completedSubmissions
                .Where(s => establishmentIds.Contains(s.EstablishmentId))
                .GroupBy(s => s.SectionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => s.EstablishmentId).Distinct().Count()
                );

            var categoryLandingSections = category.Sections
                .Select(section =>
                {
                    var completedCount = completedCountBySectionId.GetValueOrDefault(section.Id);

                    var hasSubmittedAssessments = completedCount > 0;

                    var recommendations = hasSubmittedAssessments
                        ? BuildMatSectionRecommendations(section)
                        : null;

                    return new CategoryLandingSectionViewModel(
                        section,
                        recommendations,
                        sectionStatus: null,
                        hadRetrievalError: false,
                        establishmentName: null
                    )
                    {
                        HasSubmittedAssessments = hasSubmittedAssessments,
                        HasOutstandingAssessments = completedCount < establishmentIds.Length,
                        OutstandingAssessmentCount = establishmentIds.Length - completedCount,
                    };
                })
                .ToList();

            return new CategoryLandingViewComponentViewModel
            {
                CategoryName = category.Header.Text,
                CategorySlug = slug,
                Sections = category.Sections,
                SectionName = sectionName,
                CategoryLandingSections = categoryLandingSections,
                CompletedSectionsCount = categoryLandingSections.Count(x => x.HasSubmittedAssessments),
                StatusLinkPartialName = CategoryLandingSectionAssessmentLink,
                Context = CategoryLandingContext.MAT,
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An exception occurred while retrieving MAT recommendation data for category {CategorySlug}",
                slug
            );

            return new CategoryLandingViewComponentViewModel
            {
                CategoryName = category.Header.Text,
                CategorySlug = slug,
                Sections = category.Sections,
                SectionName = sectionName,
                CategoryLandingSections = [],
                ProgressRetrievalErrorMessage =
                    "Unable to retrieve recommendations, please refresh your browser.",
                StatusLinkPartialName = CategoryLandingSectionAssessmentLink,
                Context = CategoryLandingContext.MAT,
            };
        }
    }

    private async Task<CategoryLandingSectionRecommendationsViewModel> GetCategoryLandingSectionRecommendations(
        int establishmentId,
        QuestionnaireSectionEntry section,
        RecommendationSortOrder sortType
    )
    {
        if (!section.CoreRecommendations.Any())
        {
            return new CategoryLandingSectionRecommendationsViewModel
            {
                NoRecommendationFoundErrorMessage =
                    $"Section '{section.Name}' has no recommendations",
            };
        }

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
                    Status = sr.GetStatus(recommendations),
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
    private static CategoryLandingSectionRecommendationsViewModel BuildMatSectionRecommendations(
    QuestionnaireSectionEntry section
)
    {
        var chunks = section.CoreRecommendations
            .Select(recommendation => new RecommendationChunkViewModel
            {
                Header = recommendation.HeaderText,
                Slug = recommendation.Slug,
            })
            .ToList();

        return new CategoryLandingSectionRecommendationsViewModel
        {
            SectionName = section.Name,
            SectionSlug = section.InterstitialPage?.Slug,
            Chunks = chunks,
        };
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
