using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Options;

public class TextRendererOptions
{
    private readonly List<MarkOptions> _markOptions;

    public TextRendererOptions(List<MarkOptions> markOptions)
    {
        _markOptions = markOptions;
    }

    public string? GetHtmlForMark(RichTextMark mark)
    {
        var matchingOption = _markOptions.FirstOrDefault(option => option.Mark == mark.Type);

        if (matchingOption == null)
        {
            //TODO: LOG missing mark
            return null;
        }

        return $"{matchingOption.HtmlTag}{(matchingOption.Classes != null ? $" class=\"{matchingOption.Classes}\"" : "")}";
    }
}
