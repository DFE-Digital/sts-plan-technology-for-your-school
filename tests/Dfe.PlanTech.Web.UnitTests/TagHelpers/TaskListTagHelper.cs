using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests;

public class TaskListTagHelperTests
{
    [Fact]
    public void Should_Set_Variables()
    {
        var taskListItemNameTagHelper = new TaskListTagHelper();

        Assert.NotNull(taskListItemNameTagHelper.Class);
        Assert.NotNull(taskListItemNameTagHelper.TagName);
    }
}
