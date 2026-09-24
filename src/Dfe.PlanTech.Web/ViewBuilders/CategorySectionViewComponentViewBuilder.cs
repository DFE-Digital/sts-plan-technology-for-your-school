using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.ViewBuilders;

public class CategorySectionViewComponentViewBuilder(
    ILogger<BaseViewBuilder> logger,
    IContentfulService contentfulService,
    ICurrentUserProvider currentUser,
    ISubmissionService submissionService,
    IEstablishmentService establishmentService,
    IGroupService groupService
)
    : BaseViewBuilder(logger, contentfulService, currentUser),
        ICategorySectionViewComponentViewBuilder
{
    private readonly ISubmissionService _submissionService =
        submissionService ?? throw new ArgumentNullException(nameof(submissionService));
    private readonly IEstablishmentService _establishmentService =
        establishmentService ?? throw new ArgumentNullException(nameof(establishmentService));
    private readonly IGroupService _groupService =
        groupService ?? throw new ArgumentNullException(nameof(groupService));

    public async Task<CategoryCardsViewComponentViewModel> BuildViewModelAsync(
        QuestionnaireCategoryEntry category,
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
            CategoryLandingContext.MAT => await BuildMatViewModelAsync(category),
            _ => await BuildSchoolViewModelAsync(category),
        };
    }

    private async Task<CategoryCardsViewComponentViewModel> BuildSchoolViewModelAsync(
        QuestionnaireCategoryEntry category
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

        var categoryLandingSlug = GetLandingPageSlug(category);
        var description = category.Content is { Count: > 0 } content
            ? content[0]
            : new MissingComponentEntry();

        return new CategoryCardsViewComponentViewModel
        {
            CategoryHeaderText = category.Header.Text,
            CategorySlug = categoryLandingSlug,
            CompletedSectionCount = sectionStatuses.Count(ss => ss.LastCompletionDate.HasValue),
            Description = description,
            ProgressRetrievalErrorMessage = progressRetrievalErrorMessage,
            TotalSectionCount = category.Sections.Count,
            Context = CategoryLandingContext.School,
        };
    }

    private async Task<CategoryCardsViewComponentViewModel> BuildMatViewModelAsync(
        QuestionnaireCategoryEntry category
    )
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
            await _establishmentService.GetEstablishmentsByReferencesAsync(establishmentUrns) ?? [];

        var establishmentIds = establishments
            .Select(e => e.Id)
            .Distinct()
            .ToArray();

        var completedSubmissions =
            establishmentIds.Length != 0
                ? await _groupService.GetGroupCompletedSubmissionsBySections(establishmentIds) ?? []
                : [];

        var categorySectionIds = category.Sections
            .Select(section => section.Id)
            .ToHashSet();

        var completedSectionCount = completedSubmissions
            .Where(submission =>
                establishmentIds.Contains(submission.EstablishmentId)
                && categorySectionIds.Contains(submission.SectionId)
            )
            .Select(submission => submission.SectionId)
            .Distinct()
            .Count();

        var categoryLandingSlug = GetLandingPageSlug(category);
        var description = category.Content is { Count: > 0 } content
            ? content[0]
            : new MissingComponentEntry();

        return new CategoryCardsViewComponentViewModel
        {
            CategoryHeaderText = category.Header.Text,
            CategorySlug = categoryLandingSlug,
            CompletedSectionCount = completedSectionCount,
            Description = description,
            TotalSectionCount = category.Sections.Count,
            Context = CategoryLandingContext.MAT,
        };
    }

    private string? GetLandingPageSlug(QuestionnaireCategoryEntry category)
    {
        if (category?.LandingPage?.Slug is string slug)
        {
            return slug;
        }

        Logger.LogError(
            "Could not find category landing slug for category {CategoryInternalName}",
            category?.InternalName ?? "unknown category"
        );
        return null;
    }
}
