using Dfe.PlanTech.Web.TagHelpers.TaskList;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests;

public class TaskListItemNameTagHelperTests
{
    [Fact]
    public void Should_Set_Variables()
    {
        var taskListItemNameTagHelper = new TaskListItemNameTagHelper();

        Assert.NotNull(taskListItemNameTagHelper.Class);
        Assert.NotNull(taskListItemNameTagHelper.TagName);
    }
}
