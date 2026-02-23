using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration;

[ExcludeFromCodeCoverage]
public record ContentfulOptionsConfiguration(bool UsePreviewApi)
{
    public ContentfulOptionsConfiguration()
        : this(false) { }
}
