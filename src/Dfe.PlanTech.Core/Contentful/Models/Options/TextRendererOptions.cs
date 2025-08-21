using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Core.Contentful.Models.Options;

public class TextRendererOptions(
    ILoggerFactory loggerFactory,
    List<MarkOption> markOptions
)
{
    private readonly ILogger<TextRendererOptions> _logger = loggerFactory.CreateLogger<TextRendererOptions>();
    private readonly List<MarkOption> _markOptions = markOptions ?? throw new ArgumentNullException(nameof(markOptions));

    public MarkOption? GetMatchingOptionForMark(RichTextMarkField mark)
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
