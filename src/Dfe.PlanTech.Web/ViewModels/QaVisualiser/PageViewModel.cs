using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class PageViewModel
    {
        public string InternalName { get; init; } = null!;
        public string Slug { get; init; } = null!;
        public bool DisplayBackButton { get; init; }
        public bool DisplayHomeButton { get; init; }
        public bool DisplayTopicTitle { get; init; }
        public bool DisplayOrganisationName { get; init; }
        public bool RequiresAuthorisation { get; init; } = true;
        public string? SectionTitle { get; set; }
        public List<ContentComponent> BeforeTitleContent { get; init; } = [];
        public TitleViewModel? Title { get; init; }
        public string? OrganisationName { get; set; }
        public List<ContentComponent> Content { get; init; } = [];
    }
}
