using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class ViewAnswersViewModel
{
    public DateTime AssessmentCompletedDate { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public List<QuestionWithAnswerModel> Responses { get; set; } = [];
    public string CategorySlug { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public bool IsMatInProgressView { get; set; }
    public string BackLinkHref { get; set; } = "/";
    public string? SchoolName { get; set; }
    public string? StartedBySchoolName { get; set; }
    public DateTime? DateStarted { get; set; }
    public int QuestionsAnswered { get; set; }
    public int TotalQuestions { get; set; }
    public bool ShowInProgressDisclaimer { get; set; }
    public string BackButtonText { get; set; } = string.Empty;
}
