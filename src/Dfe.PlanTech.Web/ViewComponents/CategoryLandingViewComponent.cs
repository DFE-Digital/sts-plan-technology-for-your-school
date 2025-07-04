using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.CategoryLanding;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class CategoryLandingViewComponent(
    ILogger<CategoryLandingViewComponent> logger,
    IGetSubmissionStatusesQuery query,
    IGetSubTopicRecommendationQuery getSubTopicRecommendationQuery) : ViewComponent
{
    private readonly ILogger<CategoryLandingViewComponent> _logger = logger;
    private readonly IGetSubmissionStatusesQuery _query = query;
    private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery = getSubTopicRecommendationQuery;

    public async Task<IViewComponentResult> InvokeAsync(Category category)
    {
        var viewModel = await GenerateViewModel(category);

        return View(viewModel);
    }

    private async Task<CategoryLandingViewComponentViewModel> GenerateViewModel(Category category)
    {
        if (category is null)
        {
            _logger.LogInformation("Could not find category at {Path}", Request.Path.Value);
            throw new ContentfulDataUnavailableException($"Could not find category at {Request.Path.Value}");
        }

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
            Sections = category.Sections,
            AllSectionsCompleted = category.Completed == category.Sections.Count,
            AnySectionsCompleted = category.Completed > 0,
            CategoryLandingSections = await GetCategoryLandingSections(category).ToListAsync(),
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

            var recommendations = await GetCategoryLandingSectionRecommendations(section, sectionStatus);

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

    private async Task<CategoryLandingSectionRecommendations> GetCategoryLandingSectionRecommendations(Section section, SectionStatusDto? sectionStatus)
    {
        var recommendations = await _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id) ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for:  {section.Name}");


        if (string.IsNullOrEmpty(sectionStatus?.LastMaturity))
            return new CategoryLandingSectionRecommendations();

        try
        {
            var recommendation = await _getSubTopicRecommendationQuery.GetRecommendationsViewDto(section.Sys.Id, sectionStatus.LastMaturity);
            if (recommendation == null)
            {
                return new CategoryLandingSectionRecommendations
                {
                    NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
                };
            }
            return new CategoryLandingSectionRecommendations
            {
                SectionName = section.Name,
                SectionSlug = section.InterstitialPage.Slug,
                Answers = new List<QuestionWithAnswer>(),
                Chunks = recommendations.Section.Chunks,

            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                             "An exception has occurred while trying to retrieve the recommendation for Section {sectionName}, with the message {errMessage}",
                             section.Name,
                             e.Message);
            return new CategoryLandingSectionRecommendations
            {
                NoRecommendationFoundErrorMessage = $"Unable to retrieve {section.Name} recommendation"
            };
        }
    }
}
