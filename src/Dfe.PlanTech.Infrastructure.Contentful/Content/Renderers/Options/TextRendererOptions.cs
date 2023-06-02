using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Options;

public class TextRendererOptions
{
    private readonly List<MarkOption> _markOptions;

    public TextRendererOptions(List<MarkOption> markOptions)
    {
        _markOptions = markOptions;
    }

    public MarkOption? GetMatchingOptionForMark(RichTextMark mark)
    {
        var matchingOption = _markOptions.FirstOrDefault(option => option.Mark == mark.Type);

        if (matchingOption == null)
        {
            //TODO: LOG missing mark
        }

        return matchingOption;
    }

    public IEnumerable<string> GetOpenTagHtml(MarkOption option)
    {
        yield return option.HtmlTag;

        if (option.Classes != null)
        {
            yield return " class=\"";
            yield return option.Classes;
            yield return "\"";
        }
    }
}