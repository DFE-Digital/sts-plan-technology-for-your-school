namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public class TaskListTagHelper : BaseTaskListTagHelper
{
    private const string _class = "app-task-list__items";

    public TaskListTagHelper()
    {
        Class = _class;
        TagName = "ul";
    }
}
