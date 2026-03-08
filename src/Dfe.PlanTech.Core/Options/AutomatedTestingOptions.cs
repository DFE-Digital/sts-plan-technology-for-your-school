using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Options
{
    [ExcludeFromCodeCoverage]
    public class AutomatedTestingOptions
    {
        public ContentfulOptions? Contentful { get; init; }
        public MockAuthenticationOptions? MockAuthentication { get; init; }

        public class ContentfulOptions
        {
            public bool IncludeTaggedContent { get; init; }
            public string? Tag { get; init; }
        }

        public class MockAuthenticationOptions
        {
            public string? ClientSecret { get; init; }
        }
    }
}
