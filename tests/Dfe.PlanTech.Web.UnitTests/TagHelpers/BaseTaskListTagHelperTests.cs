using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests;

public static class Constants
{
    public const string TAG_NAME = "TagName";
    public const string CLASS = "TaskListClass";
}

public class BaseTaskListTagHelperTests
{
    [Fact]
    public void Should_Create_Classes_Attribute()
    {
        string classes = "TestingClasses";

        var tagHelper = new ConcreteTaskListTagHelper()
        {
            Classes = classes
        };

        var attribute = tagHelper.CreateClasses();

        var expected = $"{Constants.CLASS} {classes}";

        Assert.Equal(expected, attribute);
    }
}

public class ConcreteTaskListTagHelper : BaseTaskListTagHelper
{
    public ConcreteTaskListTagHelper()
    {
        Class = Constants.CLASS;
        TagName = Constants.TAG_NAME;
    }

    public string CreateClasses() => CreateClassesAttribute();
}
