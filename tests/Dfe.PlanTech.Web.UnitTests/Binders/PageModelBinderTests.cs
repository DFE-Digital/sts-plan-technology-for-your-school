using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Binders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Binders;

public class PageModelBinderTests
{
    private static DefaultModelBindingContext NewBindingContext(HttpContext http)
    {
        var actionContext = new ActionContext(
            http,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        return new DefaultModelBindingContext
        {
            ActionContext = actionContext,
            ModelName = "page",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(PageEntry)),
            FieldName = "page"
        };
    }

    [Fact]
    public async Task BindModelAsync_Throws_When_BindingContext_Null()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var binder = new PageModelBinder(logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => binder.BindModelAsync(null!));
    }

    [Fact]
    public async Task BindModelAsync_When_Item_Missing_Logs_Error_And_Fails()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var binder = new PageModelBinder(logger);

        var http = new DefaultHttpContext(); // no Items[nameof(PageEntry)]
        var ctx = NewBindingContext(http);

        await binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet == false);
        Assert.True(ctx.Result == ModelBindingResult.Failed());

        logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Error, 0, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task BindModelAsync_When_Item_Wrong_Type_Logs_Error_And_Fails()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var binder = new PageModelBinder(logger);

        var http = new DefaultHttpContext();
        http.Items[nameof(PageEntry)] = "not-a-page"; // wrong type

        var ctx = NewBindingContext(http);

        await binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet == false);
        Assert.True(ctx.Result == ModelBindingResult.Failed());

        logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Error, 0, Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task BindModelAsync_When_Item_Is_PageEntry_Succeeds_And_Returns_Model()
    {
        var logger = Substitute.For<ILogger<PageModelBinder>>();
        var binder = new PageModelBinder(logger);

        var page = new PageEntry { RequiresAuthorisation = false };
        var http = new DefaultHttpContext();
        http.Items[nameof(PageEntry)] = page;

        var ctx = NewBindingContext(http);

        await binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet);
        var (isModelSet, model) = (ctx.Result.IsModelSet, ctx.Result.Model);
        Assert.True(isModelSet);
        Assert.Same(page, model);
    }
}
