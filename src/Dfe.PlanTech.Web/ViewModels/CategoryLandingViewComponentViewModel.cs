using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryLandingViewComponentViewModel
{
    public string CategoryName { get; set; } = null!;

    public string CategorySlug { get; set; } = null!;

    public List<CmsQuestionnaireSectionDto> Sections { get; set; } = [];

    public List<CategoryLandingSectionViewModel> CategoryLandingSections { get; init; } = null!;

    public bool AllSectionsCompleted { get; init; }

    public bool AnySectionsCompleted { get; init; }

    public string? SectionName { get; set; }

    public string? NoSectionsErrorRedirectUrl { get; set; }

    public string? ProgressRetrievalErrorMessage { get; set; }
}
