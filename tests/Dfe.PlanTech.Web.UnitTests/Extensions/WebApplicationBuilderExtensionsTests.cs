using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
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
            typeof(ICacheService<List<CsPage>>),
            typeof(IModelMapper),
            typeof(ILayoutService)
        };
        foreach (var type in types)
            builder.Services.Where(o => o.ServiceType == type).Should().ContainSingle();
    }
}
