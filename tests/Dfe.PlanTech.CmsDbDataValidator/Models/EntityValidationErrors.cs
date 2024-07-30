using System.Text;

namespace Dfe.PlanTech.CmsDbDataValidator.Models;

public class EntityValidationErrors
{
    public string EntityId { get; init; } = null!;

    public List<DataValidationError> Errors { get; init; } = [];

    public bool HasErrors => Errors.Count != 0;

    public void AddErrorsToTable(StringBuilder stringBuilder)
    {
        foreach (var error in Errors)
        {
            stringBuilder.AppendLine($"| {EntityId} | {error.Field} | {error.Message} |");
        }
    }
}
