namespace Dfe.PlanTech.Application.Rendering.Markdown;

public sealed class RenderContext
{
    public List<string> ListStack { get; } = [];
    public List<int> ListNestingLevels { get; } = [];
}
