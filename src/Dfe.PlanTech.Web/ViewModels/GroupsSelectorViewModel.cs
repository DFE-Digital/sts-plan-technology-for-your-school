using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Web.ViewModels;
{
    public class GroupsSelectorViewModel
    {
        public string GroupName { get; set; } = null!;

        public List<SqlEstablishmentLinkDto> GroupEstablishments { get; set; } = null!;

        public string Title { get; init; } = null!;

        public List<CmsEntryDto> Content { get; init; } = null!;

        public string? ErrorMessage { get; set; }

        public string? TotalSections { get; set; }

        public string? ProgressRetrievalErrorMessage { get; init; }

        public string? ContactLinkHref { get; set; }
    }
}
