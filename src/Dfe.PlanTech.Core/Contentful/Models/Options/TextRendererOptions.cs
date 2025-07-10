using Dfe.PlanTech.Core.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Domain.Content.Models.Options;

public class TextRendererOptions
{
    private readonly ILogger<TextRendererOptions> _logger;
    private readonly List<MarkOption> _markOptions;

    public TextRendererOptions(ILogger<TextRendererOptions> logger, List<MarkOption> markOptions)
    {
        _logger = logger;
        _markOptions = markOptions;
    }

    public MarkOption? GetMatchingOptionForMark(RichTextMark mark)
    {
        var matchingOption = _markOptions.Find(option => option.Mark == mark.Type);

        if (matchingOption == null)
        {
            _logger.LogWarning("Missing mark option for {mark}", mark);
        }

        return matchingOption;
    }

    public static IEnumerable<string> GetOpenTagHtml(MarkOption option)
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
