using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public abstract class BaseTaskListTagHelper : TagHelper
{
    [HtmlAttributeName("class")]
    public string? Classes { get; set; }

    public required string Class { get; init; }

    public required string TagName { get; init; }

    protected BaseTaskListTagHelper(string? classes, string @class, string tagName)
    {
        Classes = classes;
        Class = @class;
        TagName = tagName;
    }

    protected BaseTaskListTagHelper()
    {
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = TagName;

        string classes = CreateClassesAttribute();
        output.Attributes.SetAttribute("class", classes);
    }

    protected virtual string CreateClassesAttribute() => $"{Class} {Classes ?? ""}";
}