using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Options;

namespace Dfe.PlanTech.Core.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AutomatedTestingConfiguration
    {
        public ContentfulOptions? Contentful { get; init; }
        public MockAuthenticationOptions? MockAuthentication { get; init; }

        public class ContentfulOptions
        {
            public bool IncludeTaggedContent { get; init; }
            public string? Tag { get; init; }
        }
    }
}
