using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class ServiceExceptionHandlerMiddlewareTests
    {
        [Fact]
        public void Should_Get_Error_Redirect_On_Null_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/error", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public void Should_Get_Service_Unavailable_Redirect_ContentfulDataUnavailableException_Exception()
        {
            // Arrange
            var exception = new ContentfulDataUnavailableException("service-unavailable exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/service-unavailable", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public void Should_Get_Not_Found_Redirect_PageNotFound_Exception()
        {
            // Arrange
            var exception = new PageNotFoundException("not-found exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/not-found", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public void Should_Get_Service_Unavailable_Redirect_DatabaseException_Exception()
        {
            // Arrange
            var exception = new DatabaseException("service-unavailable exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/service-unavailable", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public void Should_Get_Service_Unavailable_Redirect_InvalidEstablishmentException_Exception()
        {
            // Arrange
            var exception = new InvalidEstablishmentException("service-unavailable exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/service-unavailable", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public void Should_Get_OrgError_Redirect_KeyNotFoundException_Exception()
        {
            // Arrange
            var exception = new KeyNotFoundException("organisation exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);
            var middleware = new ServiceExceptionHandlerMiddleWare();

            // Act
            middleware.ContextRedirect(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal(UrlConstants.OrgErrorPage, context.Response.Headers.Values.FirstOrDefault());
        }
    }
}
