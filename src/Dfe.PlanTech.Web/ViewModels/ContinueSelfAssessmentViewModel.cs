using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class ContinueSelfAssessmentViewModel
    {
        public string TrustName { get; set; } = string.Empty;
        public DateTime AssessmentStartDate { get; set; }
        public int AnsweredCount { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public int QuestionsCount { get; set; }
        public List<QuestionWithAnswerModel> Responses { get; set; } = new();

        public string CategorySlug { get; set; } = string.Empty;
        public string SectionSlug { get; set; } = string.Empty;
    }
}
