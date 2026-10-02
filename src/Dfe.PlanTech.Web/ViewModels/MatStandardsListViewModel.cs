using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class MatStandardsListViewModel
    {
        public required string GroupName { get; init; }
        public required List<QuestionnaireCategoryEntry> Categories { get; init; }
    }
}
