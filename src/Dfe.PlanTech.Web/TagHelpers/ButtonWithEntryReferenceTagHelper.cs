using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using GovUk.Frontend.AspNetCore.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers;

public class ButtonWithEntryReferenceTagHelper : TagHelper
{
    private const string HTML_TAG = "govuk-button-link";

    private readonly ILogger<ButtonWithEntryReferenceTagHelper> _logger;

    public ButtonWithEntryReference? Model { get; set; }

    public ButtonWithEntryReferenceTagHelper(ILogger<ButtonWithEntryReferenceTagHelper> logger)
    {
        _logger = logger;
    }

    public override async void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Model == null)
        {
            _logger.LogWarning($"Missing {nameof(Model)}");
            return;
        }

        var html = GetHtml();

        output.Content.SetHtmlContent(html);
        
        var tagHelper = new ButtonLinkTagHelper();
        tagHelper.IsStartButton = Model!.Button.IsStartButton;
        await tagHelper.ProcessAsync(context, output);
    }

    public string GetHtml()
    {
        var stringBuilder = new StringBuilder();
        AppendOpenTag(stringBuilder);
        stringBuilder.Append(Model!.Button.Value);
        AppendCloseTag(stringBuilder);

        return stringBuilder.ToString();
    }

    private StringBuilder AppendCloseTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append("</");
        stringBuilder.Append(HTML_TAG);
        stringBuilder.Append('>');

        return stringBuilder;
    }

    private StringBuilder AppendOpenTag(StringBuilder stringBuilder)
    {
        stringBuilder.Append('<');
        stringBuilder.Append(HTML_TAG);

        switch (Model!.LinkToEntry)
        {
            case Question question:
                {
                    stringBuilder.Append(" QUESTION ");
                    break;
                }
            case Page page:
                {
                    stringBuilder.Append(" PAGE ");
                    break;
                }
            default:
                {
                    stringBuilder.Append(Model.LinkToEntry.GetType());
                    break;
                }
        }

        stringBuilder.Append('>');
        return stringBuilder;
    }
}
