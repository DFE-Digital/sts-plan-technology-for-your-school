using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RecommendationChunkEntry : ContentfulEntry, IHeaderWithContent
{
    public string InternalName { get; set; } = null!;
    public string Header { get; init; } = null!;
    public List<ContentfulEntry> Content { get; init; } = [];
    public QuestionnaireQuestionEntry Question { get; set; } = null!;
    public List<QuestionnaireAnswerEntry> CompletingAnswers { get; init; } = [];
    public List<QuestionnaireAnswerEntry> InProgressAnswers { get; init; } = [];
    public CAndSLinkEntry? CSLink { get; init; }

    public List<QuestionnaireAnswerEntry> AllAnswers => CompletingAnswers.Union(InProgressAnswers).DistinctBy(a => a.Id).ToList();
    public string HeaderText => Header;
    public string LinkText => HeaderText;

    private string? _slugifiedLinkText;
    public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();
}
