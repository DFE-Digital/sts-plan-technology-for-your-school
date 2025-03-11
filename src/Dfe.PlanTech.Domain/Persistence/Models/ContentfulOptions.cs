namespace Dfe.PlanTech.Domain.Persistence.Models;

public record ContentfulOptions(bool UsePreviewApi)
{
    public ContentfulOptions() : this(false)
    {
    }
}
