
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Web.Models
{
    public class HeaderContentViewModel
    {
        public IHeaderWithContent? Header { get; set; }
        public string? SubmissionDate { get; set; }
        public string? SectionName { get; set; }
    }
}
