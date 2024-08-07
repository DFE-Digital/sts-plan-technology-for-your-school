using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsChecklistViewModel
{
    public RecommendationIntro Intro { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    private static RecommendationChunk ConvertToNumberedChunk(RecommendationChunk content, int index)
    => new()
    {
        Header = new Header
        {
            Text = $"{index + 1}. {content.Header.Text}",
            Tag = HeaderTag.H3,
            Size = HeaderSize.Medium
        },
        Content = content.Content,
    };

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        yield return Intro;
        foreach (var numberedChunk in Chunks.Select(ConvertToNumberedChunk))
        {
            yield return numberedChunk;
        }
    }
}
