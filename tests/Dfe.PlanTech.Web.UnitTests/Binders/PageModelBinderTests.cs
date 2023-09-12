using Azure;
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
  public void Should_Get_Page_From_Items()
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

    pageModelBinder.BindModelAsync(modelBinderContext);

    Assert.True(modelBinderContext.Result.IsModelSet);
    Assert.Equal(page, modelBinderContext.Result.Model);
  }
}