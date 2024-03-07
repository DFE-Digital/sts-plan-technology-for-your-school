using Dfe.PlanTech.Web.Helpers;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class ViewHelperTests
{

  [Theory]
  [InlineData("Random test Topic", "random-test-topic")]
  [InlineData("Y867as ()&ycj Cool Thing", "y867as-ycj-cool-thing")]
  public void Should_Slugify_Strings(string toSlugify, string expected)
  {
    var slugified = ViewHelpers.Slugify(toSlugify);

    Assert.Equal(expected, slugified);
  }
}