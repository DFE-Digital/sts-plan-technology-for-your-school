using Dfe.PlanTech.Web.TagHelpers.TaskList;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers.TaskList;

public class TaskListItemTagHelperTests
{
    [Fact]
    public void Should_Set_Variables()
    {
        var taskListItemNameTagHelper = new TaskListItemTagHelper();

        Assert.NotNull(taskListItemNameTagHelper.Class);
        Assert.NotNull(taskListItemNameTagHelper.TagName);
    }
}
