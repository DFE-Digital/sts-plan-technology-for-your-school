using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.ViewComponents;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategoryLandingViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    ContentfulService contentfulService,
    CurrentUser currentUser,
    SubmissionService submissionService
) : BaseViewBuilder(logger, contentfulService, currentUser)
{
    private readonly SubmissionService _submissionService = submissionService ?? throw new ArgumentNullException(nameof(submissionService));

    public async Task<CategoryLandingViewComponentViewModel> BuildViewModelAsync(QuestionnaireCategoryEntry category, string slug, string? sectionName)
    {
        if (!category.Sections.Any())
        {
            Logger.LogError("Found no sections for category {id}", category.Id);
            throw new InvalidDataException($"Found no sections for category {category.Id}");
        }

        var establishmentId = GetEstablishmentIdOrThrowException();

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

        var categoryLandingSections = await BuildCategoryLandingSectionViewModels(establishmentId, category, sectionStatuses, progressRetrievalErrorMessage is not null).ToListAsync();
        var completedSectionCount = sectionStatuses.Count(ss => ss.LastCompletionDate != null);

        var viewModel = new CategoryLandingViewComponentViewModel()
        {
            AllSectionsCompleted = completedSectionCount.Equals(category.Sections.Count),
            AnySectionsCompleted = completedSectionCount > 0,
            CategoryLandingSections = categoryLandingSections,
            CategoryName = category.Header.Text,
            CategorySlug = slug,
            Sections = category.Sections,
            SectionName = sectionName,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage
        };

        return viewModel;
    }

    private async IAsyncEnumerable<CategoryLandingSectionViewModel> BuildCategoryLandingSectionViewModels(
        int establishmentId,
        QuestionnaireCategoryEntry category,
        List<SqlSectionStatusDto> sectionStatuses,
        bool hadRetrievalError
    )
    {
        foreach (var section in category.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
            {
                Logger.LogError("No slug found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var sectionStatus = sectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId.Equals(section.Id));
            if (sectionStatus is null)
            {
                Logger.LogError("No section status found for subtopic with ID {sectionId} and name {sectionName}", section.Id, section.Name);
            }

            var recommendations = await GetCategoryLandingSectionRecommendations(establishmentId, section, sectionStatus);

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
        SqlSectionStatusDto? sectionStatus
    )
    {
        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
        {
            return new CategoryLandingSectionRecommendationsViewModel();
        }

        try
        {
            var recommendationIntro = await ContentfulService.GetSubtopicRecommendationIntroAsync(section.Id, sectionStatus.LastMaturity);
            if (recommendationIntro == null)
            {
                return new CategoryLandingSectionRecommendationsViewModel
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }

            var latestResponses = await _submissionService.GetLatestSubmissionResponsesModel(establishmentId, section, true)
                ?? throw new DatabaseException($"Could not find user's answers for section {section.Name}");
            var subtopicRecommendation = await ContentfulService.GetSubtopicRecommendationByIdAsync(section.Id)
               ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for section {section.Name}");

            var subTopicChunks = subtopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Responses.Select(answer => answer.AnswerSysId));

            if (section.InterstitialPage is null)
            {
                throw new ContentfulDataUnavailableException($"Could not find {section.Name} interstitial page");
            }

            return new CategoryLandingSectionRecommendationsViewModel
            {
                SectionName = section.Name,
                SectionSlug = section.InterstitialPage.Slug,
                Answers = latestResponses.Responses,
                Chunks = subTopicChunks,
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
