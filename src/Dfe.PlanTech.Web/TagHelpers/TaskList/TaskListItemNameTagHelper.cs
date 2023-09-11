namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public class TaskListItemNameTagHelper : BaseTaskListTagHelper
{
    private const string _class = "app-task-list__task-name";

    public TaskListItemNameTagHelper()
    {
        Class = _class;
        TagName = "span";
    }
}
