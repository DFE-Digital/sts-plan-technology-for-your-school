using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.CategoryLanding;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategoryLandingViewComponent(
    ILogger<CategoryLandingViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery,
    IGetLatestResponsesQuery getLatestResponsesQuery,
    IUser user) : ViewComponent
{
    private readonly ILogger<CategoryLandingViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;
    private readonly IGetLatestResponsesQuery _getLatestResponsesQuery = getLatestResponsesQuery;
    private readonly IUser _user = user;

    public async Task<IViewComponentResult> InvokeAsync(Category category, string slug, string? sectionName = null)
    {
        var viewModel = await GenerateViewModel(category, slug, sectionName);

        return View(viewModel);
    }

    private async Task<CategoryLandingViewComponentViewModel> GenerateViewModel(Category category, string slug, string? sectionName = null)
    {
        if (category.Sections.Count == 0)
        {
            _logger.LogError("Found no sections for category {id}", category.Sys.Id);
            return new CategoryLandingViewComponentViewModel
            {
                NoSectionsErrorRedirectUrl = "ServiceUnavailable"
            };
        }

        category = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _query);

        var viewModel = new CategoryLandingViewComponentViewModel()
        {
            CategoryName = category.Header.Text,
            CategorySlug = slug,
            Sections = category.Sections,
            AllSectionsCompleted = category.Completed == category.Sections.Count,
            AnySectionsCompleted = category.Completed > 0,
            CategoryLandingSections = await GetCategoryLandingSections(category).ToListAsync(),
            SectionName = sectionName,
            ProgressRetrievalErrorMessage = category.RetrievalError
                ? "Unable to retrieve progress, please refresh your browser."
                : null
        };

        return viewModel;
    }

    private async IAsyncEnumerable<CategoryLandingSection> GetCategoryLandingSections(Category category)
    {
        foreach (var section in category.Sections)
        {
            var sectionStatus = category.SectionStatuses.FirstOrDefault(sectionStatus => sectionStatus.SectionId == section.Sys.Id);

            if (string.IsNullOrWhiteSpace(section.InterstitialPage?.Slug))
                _logger.LogError("No Slug found for Subtopic with ID: {sectionId}/ name: {sectionName}", section.Sys.Id, section.Name);

            var recommendations = await GetCategoryLandingSectionRecommendations(section);

            yield return new CategoryLandingSection(
                slug: section.InterstitialPage?.Slug,
                name: section.Name,
                shortDescription: section.ShortDescription,
                retrievalError: category.RetrievalError,
                sectionStatus,
                recommendations
            );
        }
    }

    private async Task<CategoryLandingSectionRecommendations> GetCategoryLandingSectionRecommendations(Section section)
    {
        try
        {
            var submissionResponses = await _getLatestResponsesQuery.GetLatestResponses(await _user.GetEstablishmentId(), section.Sys.Id, true) ?? throw new DatabaseException($"Could not find users answers for:  {section.Name}");
            var latestResponses = section.GetOrderedResponsesForJourney(submissionResponses.Responses).ToList();
            var subTopicRecommendation = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {section.Name}");
            var subTopicChunks = subTopicRecommendation.Section.GetRecommendationChunksByAnswerIds(latestResponses.Select(answer => answer.AnswerRef));

            if (section.InterstitialPage == null)
            {
                throw new ContentfulDataUnavailableException($"Could not find {section.Name} interstitial page");
            }

            return new CategoryLandingSectionRecommendations
            {
                SectionName = section.Name,
                SectionSlug = section.InterstitialPage.Slug,
                Answers = latestResponses,
                Chunks = subTopicChunks,
            };
        }
        catch
        {
            return new CategoryLandingSectionRecommendations
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }
}
