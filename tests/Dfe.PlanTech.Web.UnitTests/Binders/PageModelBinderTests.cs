using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Binders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Binders;

public class PageModelBinderTests
{
    [Fact]
    public async Task Should_Get_Page_From_Items()
    {
        var page = new Page()
        {
            Slug = "Testing Slug"
        };

        var pageModelBinder = new PageModelBinder(new NullLoggerFactory().CreateLogger<PageModelBinder>());

        var modelBinderContext = Substitute.For<ModelBindingContext>();
        modelBinderContext.Result = new ModelBindingResult();

        var httpContext = Substitute.For<HttpContext>();

        httpContext.Items = new Dictionary<object, object?>
        {
            [nameof(Page)] = page
        };

        modelBinderContext.HttpContext.Returns(httpContext);

        await pageModelBinder.BindModelAsync(modelBinderContext);

        Assert.True(modelBinderContext.Result.IsModelSet);
        Assert.Equal(page, modelBinderContext.Result.Model);
    }

    [Fact]
    public async Task Should_LogError_When_Page_Not_Expected_Type()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var pageModelBinder = new PageModelBinder(logger);

        var modelBinderContext = Substitute.For<ModelBindingContext>();
        modelBinderContext.Result = new ModelBindingResult();

        var httpContext = Substitute.For<HttpContext>();

        httpContext.Items = new Dictionary<object, object?>
        {
            [nameof(Page)] = "Not a page type"
        };

        modelBinderContext.HttpContext.Returns(httpContext);

        await pageModelBinder.BindModelAsync(modelBinderContext);

        Assert.Single(logger.GetMatchingReceivedMessages($"Page is not {typeof(Page)}", LogLevel.Error));
        Assert.False(modelBinderContext.Result.IsModelSet);
    }

    [Fact]
    public async Task Should_LogError_When_Page_Not_Present()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var pageModelBinder = new PageModelBinder(logger);

        var modelBinderContext = Substitute.For<ModelBindingContext>();
        modelBinderContext.Result = new ModelBindingResult();

        var httpContext = Substitute.For<HttpContext>();

        httpContext.Items = new Dictionary<object, object?>();

        modelBinderContext.HttpContext.Returns(httpContext);

        await pageModelBinder.BindModelAsync(modelBinderContext);

        Assert.Single(logger.GetMatchingReceivedMessages("Page is not set", LogLevel.Error));

        Assert.False(modelBinderContext.Result.IsModelSet);
    }

}
