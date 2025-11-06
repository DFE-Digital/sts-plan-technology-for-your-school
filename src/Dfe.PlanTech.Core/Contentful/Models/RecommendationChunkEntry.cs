using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RecommendationChunkEntry : ContentfulEntry
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

    public string Slug { get; set; } = null!;
}
