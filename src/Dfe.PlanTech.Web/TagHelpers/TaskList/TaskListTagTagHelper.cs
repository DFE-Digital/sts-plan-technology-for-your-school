using System.Text;
using Dfe.PlanTech.Domain.Constants;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public class TaskListTagTagHelper : BaseTaskListTagHelper
{
    private const string _class = "app-task-list__tag";
    private const string _colourClass = "govuk-tag";

    [HtmlAttributeName("colour")]
    public string? Colour { get; set; }

    public TaskListTagTagHelper()
    {
        Class = _class;
        TagName = "strong";
    }

    protected string TagClassColour => TagColourConstants.GetMatchingColour(Colour);

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
        stringBuilder.Append($" {_colourClass}");
        if (TagClassColour != TagColourConstants.Default)
        {
            stringBuilder.Append($" {_colourClass}--{TagClassColour}");
        }
    }
}
