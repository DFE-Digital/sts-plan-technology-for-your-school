using System.Text;

namespace Dfe.PlanTech.CmsDbDataValidator.Models;

public class EntityTypeValidationErrors
{
    public string ContentfulContentType { get; init; } = null!;

    public List<EntityValidationErrors> EntityValidationErrors { get; set; } = [];

    public override string ToString()
    {
        if (!EntityValidationErrors.Any(validationErrors => validationErrors.HasErrors))
        {
            return $"No errors for {ContentfulContentType}";
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"## {ContentfulContentType} validation errors");
        stringBuilder.AppendLine("");
        //Add columns
        stringBuilder.AppendLine("| EntityId | Field | Error | ");
        stringBuilder.AppendLine("| -------- | ----- | ----- | ");

        foreach (var entityValidationError in EntityValidationErrors)
        {
            entityValidationError.AddErrorsToTable(stringBuilder);
        }

        return stringBuilder.ToString();
    }

    public void AddErrors(EntityValidationErrors errors)
    {
        if (!errors.HasErrors)
        {
            return;
        }

        EntityValidationErrors.Add(errors);
    }
}
