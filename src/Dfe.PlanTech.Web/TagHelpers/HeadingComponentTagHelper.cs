using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

public class HeaderComponentTagHelper : TagHelper
{
    private readonly ILogger<HeaderComponentTagHelper> _logger;

    public Header? Model { get; set; }

    public HeaderComponentTagHelper(ILogger<HeaderComponentTagHelper> logger)
    {
        _logger = logger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Model == null || Model.Tag == HeaderTag.Unknown || !TryBuildElement(output))
        {
            output.TagName = null;
            output.Content.SetHtmlContent("");
            _logger.LogWarning("Missing or invalid {Name} {Model}", nameof(Header), Model);
            return;
        }

        output.TagMode = TagMode.StartTagAndEndTag;
    }

    public bool TryBuildElement(TagHelperOutput output)
    {
        var tagName = Model!.Tag.ToString().ToLower();
        var classForSize = Model.GetClassForSize();

        output.TagName = tagName;
        output.Attributes.Add("class", classForSize);
        output.Content.SetHtmlContent(Model.Text);

        return true;
    }
}
