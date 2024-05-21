using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using Dfe.PlanTech.Domain.Constants;

namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public class TaskListTagTagHelper : BaseTaskListTagHelper
{
    private const string _class = "govuk-tag";

    [HtmlAttributeName("colour")]
    public string? Colour { get; set; }

    public TaskListTagTagHelper()
    {
        Class = _class;
        TagName = "strong";
    }

    protected string TagClassColour => TagColour.GetMatchingColour(Colour);

    protected override string CreateClassesAttribute()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(_class);

        AppendTagColour(stringBuilder);
        AppendExtraClasses(stringBuilder);

        return stringBuilder.ToString();
    }

    private void AppendExtraClasses(StringBuilder stringBuilder)
    {
        if (!string.IsNullOrEmpty(Classes))
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(Classes);
        }
    }

    private void AppendTagColour(StringBuilder stringBuilder)
    {
        if (TagClassColour != TagColour.Default)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(_class);
            stringBuilder.Append("--");
            stringBuilder.Append(TagClassColour);
        }
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.Attributes.SetAttribute("style", "float: right;");
    }
}
