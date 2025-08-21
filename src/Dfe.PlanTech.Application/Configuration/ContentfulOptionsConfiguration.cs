namespace Dfe.PlanTech.Application.Configuration;

public record ContentfulOptionsConfiguration(bool UsePreviewApi)
{
    public ContentfulOptionsConfiguration() : this(false)
    {
    }
}
