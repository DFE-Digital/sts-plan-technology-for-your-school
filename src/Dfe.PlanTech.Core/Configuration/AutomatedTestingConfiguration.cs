using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AutomatedTestingConfiguration
    {
        public ContentfulOptions? Contentful { get; init; }

        public class ContentfulOptions
        {
            public bool IncludeTaggedContent { get; init; }
            public string? Tag { get; init; }
        }
    }
}
