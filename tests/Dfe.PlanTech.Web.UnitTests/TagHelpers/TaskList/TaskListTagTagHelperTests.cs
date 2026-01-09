using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers.TaskList
{
    public class TaskListTagTagHelperTests
    {
        [Fact]
        public void Should_Set_Variables()
        {
            var tagHelper = new TaskListTagTagHelper();

            Assert.NotNull(tagHelper.Class);
            Assert.NotNull(tagHelper.TagName);
        }

        [Fact]
        public void Should_Create_CorrectColour_When_ValidColour()
        {
            var colour = "grey";

            var tagHelper = new TaskListTagTagHelper { Colour = colour };

            var context = new TagHelperContext(
                tagName: "task-list-tag",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "tasklisttag-test"
            );

            var output = new TagHelperOutput(
                "task-list-tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("task list tag");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                }
            );

            tagHelper.Process(context, output);

            var classAttribute = output.Attributes["class"].Value.ToString();

            Assert.Contains(
                $"app-task-list__tag govuk-tag govuk-tag--{colour.ToLower()}",
                classAttribute
            );
            Assert.Equal("strong", output.TagName);
        }

        [Fact]
        public void Should_Create_DefaultColour_When_InvalidColour()
        {
            var colour = "Clementine";

            var tagHelper = new TaskListTagTagHelper { Colour = colour };

            var context = new TagHelperContext(
                tagName: "task-list-tag",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "tasklisttag-test"
            );

            var output = new TagHelperOutput(
                "task-list-tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("task list tag");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                }
            );

            tagHelper.Process(context, output);

            var classAttribute = output.Attributes["class"].Value.ToString();

            Assert.Contains("app-task-list__tag govuk-tag", classAttribute);
            Assert.Equal("strong", output.TagName);
        }

        [Fact]
        public void Should_Be_Strong_Tag()
        {
            var tagHelper = new TaskListTagTagHelper { Colour = "grey" };

            var context = new TagHelperContext(
                tagName: "task-list-tag",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "tasklisttag-test"
            );

            var output = new TagHelperOutput(
                "task-list-tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("task list tag");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                }
            );

            tagHelper.Process(context, output);

            Assert.Equal("strong", output.TagName);
        }
    }
}
