using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers
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

            var tagHelper = new TaskListTagTagHelper
            {
                Colour = colour
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.Contains($"class=\"app-task-list__tag govuk-tag govuk-tag--{colour.ToLower()}\"", htmlString);
        }

        [Fact]
        public void Should_Create_DefaultColour_When_InvalidColour()
        {
            var colour = "Clementine";

            var tagHelper = new TaskListTagTagHelper
            {
                Colour = colour
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.Contains("class=\"app-task-list__tag govuk-tag\"", htmlString);
        }

        [Fact]
        public void Should_Be_Strong_Tag()
        {
            var tagHelper = new TaskListTagTagHelper
            {
                Colour = "grey"
            };

            var context = new TagHelperContext(tagName: "task-list-tag",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "tasklisttag-test");

            var output = new TagHelperOutput("task-list-tag",
                                            attributes: new TagHelperAttributeList(),
                                            getChildContentAsync: (useCachedResult, encoder) =>
                                            {
                                                var tagHelperContent = new DefaultTagHelperContent();
                                                tagHelperContent.SetContent("task list tag");
                                                return Task.FromResult<TagHelperContent>(tagHelperContent);
                                            });

            tagHelper.Process(context, output);

            var htmlString = output.ToHtmlString();

            Assert.StartsWith("<strong", htmlString);
            Assert.EndsWith("</strong>", htmlString);
        }
    }
}
