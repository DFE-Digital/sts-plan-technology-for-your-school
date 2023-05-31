namespace Dfe.PlanTech.Web.TagHelpers.TaskList;

public class TaskListItemTagHelper : BaseTaskListTagHelper
{
    private const string _class = "app-task-list__item";

    public TaskListItemTagHelper()
    {
        Class = _class;
        TagName = "li";
    }
}
