using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record ContentfulOptionsConfiguration(bool UsePreviewApi)
{
    public ContentfulOptionsConfiguration()
        : this(false) { }
}
