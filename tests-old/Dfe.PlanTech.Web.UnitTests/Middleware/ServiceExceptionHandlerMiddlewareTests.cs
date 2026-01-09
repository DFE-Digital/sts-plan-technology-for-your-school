using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Web.Configurations;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class ServiceExceptionHandlerMiddlewareTests
    {
        private const string InternalErrorSlug = "/server-error";
        private ServiceExceptionHandlerMiddleware _middleware;

        public ServiceExceptionHandlerMiddlewareTests()
        {
            var internalErrorPageId = "Internal Error Page ID";
            var internalErrorPage = new Page { Slug = InternalErrorSlug };

            var errorPages = new ErrorPagesConfiguration
            {
                InternalErrorPageId = internalErrorPageId,
            };
            var errorPagesOptions = Substitute.For<IOptions<ErrorPagesConfiguration>>();
            errorPagesOptions.Value.Returns(errorPages);

            var getPageQuery = Substitute.For<IGetPageQuery>();
            getPageQuery.GetPageById(internalErrorPageId).Returns(internalErrorPage);

            _middleware = new ServiceExceptionHandlerMiddleware(errorPagesOptions, getPageQuery);
        }

        [Fact]
        public async Task Should_Get_Error_Redirect_On_Null_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _middleware.HandleExceptionAsync(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/server-error", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_Service_Unavailable_Redirect_ContentfulDataUnavailableException_Exception()
        {
            // Arrange
            var exception = new ContentfulDataUnavailableException("service-unavailable exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            // Act
            await _middleware.HandleExceptionAsync(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/server-error", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_Service_Unavailable_Redirect_DatabaseException_Exception()
        {
            // Arrange
            var exception = new DatabaseException("server-error exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            // Act
            await _middleware.HandleExceptionAsync(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/server-error", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_Service_Unavailable_Redirect_InvalidEstablishmentException_Exception()
        {
            // Arrange
            var exception = new InvalidEstablishmentException("service-unavailable exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            // Act
            await _middleware.HandleExceptionAsync(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal("/server-error", context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_OrgError_Redirect_KeyNotFoundException_Exception()
        {
            // Arrange
            var exception = new KeyNotFoundException("organisation exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            // Act
            await _middleware.HandleExceptionAsync(context);

            //Assert
            Assert.NotNull(context.Response);
            Assert.Equal(
                UrlConstants.OrgErrorPage,
                context.Response.Headers.Values.FirstOrDefault()
            );
        }
    }
}
