using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireSectionEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Name { get; init; } = null!;
    public string ShortDescription { get; init; } = null!;
    public PageEntry InterstitialPage { get; set; } = null!;
    public IEnumerable<QuestionnaireQuestionEntry> Questions { get; set; } = [];

    public QuestionnaireQuestionEntry GetQuestionBySlug(string questionSlug)
    {
        return Questions.FirstOrDefault(question => question.Slug.Equals(questionSlug))
             ?? throw new ContentfulDataUnavailableException($"Could not find question slug '{questionSlug}' under section '{Name}'");
    }
}
