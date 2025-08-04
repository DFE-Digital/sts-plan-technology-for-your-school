using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsQuestionnaireSectionDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string Name { get; init; } = null!;
    public CmsPageDto InterstitialPage { get; set; } = null!;
    public IEnumerable<CmsQuestionnaireQuestionDto> Questions { get; init; } = [];

    public CmsQuestionnaireSectionDto(QuestionnaireSectionEntry questionnaireSectionEntry)
    {
        Id = questionnaireSectionEntry.Id;
        InternalName = questionnaireSectionEntry.InternalName;
        Name = questionnaireSectionEntry.Name;
        InterstitialPage = questionnaireSectionEntry.InterstitialPage.AsDto();
        Questions = questionnaireSectionEntry.Questions.Select(q => q.AsDto()).ToList();
    }

    public CmsQuestionnaireQuestionDto GetQuestionBySlug(string questionSlug)
    {
       return Questions.FirstOrDefault(question => question.Slug.Equals(questionSlug))
            ?? throw new ContentfulDataUnavailableException($"Could not find question slug {questionSlug} under section {Name}");
    }
}
