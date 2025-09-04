using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers
{
    public class MaintainUrlOnKeyNotFoundAttributeTests
    {
        private MaintainUrlOnKeyNotFoundAttribute _attribute;

        public MaintainUrlOnKeyNotFoundAttributeTests()
        {
            var href = "contactLinkHref";

            var navigationLink = Substitute.For<INavigationLink>();
            navigationLink.Href = href;

            var getNavigationQuery = Substitute.For<IGetNavigationQuery>();
            getNavigationQuery.GetLinkById(href).Returns(navigationLink);

            var options = new ContactOptionsConfiguration
            {
                LinkId = href
            };
            var contactOptions = Substitute.For<IOptions<ContactOptionsConfiguration>>();
            contactOptions.Value.Returns(options);

            _attribute = new MaintainUrlOnKeyNotFoundAttribute(getNavigationQuery, contactOptions);
        }

        public ActionContext GetMockedActionContext()
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = Substitute.For<ActionDescriptor>();

            return new ActionContext(httpContext, routeData, actionDescriptor);
        }

        [Fact]
        public async Task OnExceptionAsync_Does_Not_Alter_Context_Result_When_Not_A_KeyNotFoundException()
        {
            // Arrange
            var actionContext = GetMockedActionContext();
            var exception = new Exception();
            var context = new ExceptionContext(actionContext, [])
            {
                Exception = exception
            };

            // Act
            await _attribute.OnExceptionAsync(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task OnExceptionAsync_Does_Not_Alter_Context_Result_When_KeyNotFoundException_Contains_Organisation()
        {
            // Arrange
            var actionContext = GetMockedActionContext();
            var exception = new KeyNotFoundException(ClaimConstants.Organisation);
            var context = new ExceptionContext(actionContext, [])
            {
                Exception = exception
            };

            // Act
            await _attribute.OnExceptionAsync(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public async Task OnExceptionAsync_Sets_New_Result_When_KeyNotFoundException_Does_Not_Contain_Organisation()
        {
            // Arrange
            var actionContext = GetMockedActionContext();
            var exception = new KeyNotFoundException();
            var context = new ExceptionContext(actionContext, [])
            {
                Exception = exception
            };

            // Act
            await _attribute.OnExceptionAsync(context);

            // Assert
            Assert.NotNull(context.Result);
        }
    }
}
