using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Extensions;

public class WebApplicationBuilderExtensionsTests
{
    [Fact]
    public void Builder_Contains_Correct_Services()
    {
        var builder = WebApplication.CreateBuilder();
        builder.InitCsDependencyInjection();

        var types = new[]
        {
            typeof(IContentService),
            typeof(IContentfulService),
            typeof(ICacheService<List<CsPage>>),
            typeof(IModelMapper),
            typeof(ILayoutService)
        };
        foreach (var type in types)
            builder.Services.Where(o => o.ServiceType == type).Should().ContainSingle();
    }

    [Fact]
    public void Builder_Default_Uses_DefaultClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.InitCsDependencyInjection();


        var service = builder.Services.First(o => o.ServiceType == typeof(IContentfulService));
        service.KeyedImplementationType?.Name.Should().BeEquivalentTo(nameof(ContentfulService));
    }

    [Fact]
    public void Builder_E2e_Uses_MockClient()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "e2e"
        });

        builder.InitCsDependencyInjection();

        var service = builder.Services.First(o => o.ServiceType == typeof(IContentfulService));
        service.KeyedImplementationType?.Name.Should()
            .BeEquivalentTo(nameof(StubContentfulService));
    }
}
