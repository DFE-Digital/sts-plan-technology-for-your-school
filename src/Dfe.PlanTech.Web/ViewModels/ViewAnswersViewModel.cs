using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class ViewAnswersViewModel
    {
        public DateTime AssessmentCompletedDate { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public List<QuestionWithAnswerModel> Responses { get; set; } = new();

        public string CategorySlug { get; set; } = string.Empty;
        public string SectionSlug { get; set; } = string.Empty;
    }
}
