using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Options
{
    [ExcludeFromCodeCoverage]

    public class AutomatedTestingOptions
    {
        public ContentfulOptions? Contentful { get; init; }

        public class ContentfulOptions
        {
            public bool IncludeTaggedContent { get; init; }
            public string? Tag { get; init; }
        }
    }
}
