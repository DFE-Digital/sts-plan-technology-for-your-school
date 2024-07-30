namespace Dfe.PlanTech.CmsDbDataValidator.Models;

public class DataValidationError(string field, string message)
{
    public string Field { get; set; } = field;

    public string Message { get; set; } = message;
}
