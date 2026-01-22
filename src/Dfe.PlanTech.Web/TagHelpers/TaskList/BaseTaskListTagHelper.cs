using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public abstract class BaseTaskListTagHelper : TagHelper
{
    [HtmlAttributeName("class")]
    public string? Classes { get; set; }

    public string Class { get; init; } = null!;

    public string TagName { get; init; } = null!;

    protected BaseTaskListTagHelper() { }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = TagName;

        string classes = CreateClassesAttribute();
        output.Attributes.SetAttribute("class", classes);
    }

    protected virtual string CreateClassesAttribute() => $"{Class} {Classes ?? ""}";
}
