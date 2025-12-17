using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class RestartObsoleteAssessmentViewModel
    {
        public string TopicName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public string SectionSlug { get; set; } = string.Empty;
    }
}
