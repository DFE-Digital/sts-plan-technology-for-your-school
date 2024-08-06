namespace Dfe.PlanTech.Domain.Persistence.Models;

public record ContentfulOptions(bool UsePreview)
{
    public ContentfulOptions() : this(false)
    {
    }
}
