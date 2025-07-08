namespace Dfe.PlanTech.Domain.Persistence.Models;

public record ContentfulOptionsConfiguration(bool UsePreviewApi)
{
    public ContentfulOptionsConfiguration() : this(false)
    {
    }
}
