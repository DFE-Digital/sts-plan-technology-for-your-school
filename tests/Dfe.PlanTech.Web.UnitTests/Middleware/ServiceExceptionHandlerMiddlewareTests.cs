using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Middleware
{
    public class ServiceExceptionHandlerMiddlewareTests
    {
        private const string InternalErrorSlug = "/server-error";
        private readonly ServiceExceptionHandlerMiddleware _middleware;

        public ServiceExceptionHandlerMiddlewareTests()
        {
            var internalErrorPageId = "Internal Error Page ID";

            var errorPages = new ErrorPagesConfiguration { InternalErrorPageId = internalErrorPageId };
            var errorPagesOptions = Substitute.For<IOptions<ErrorPagesConfiguration>>();
            errorPagesOptions.Value.Returns(errorPages);

            var contentfulService = Substitute.For<IContentfulService>();

            var internalErrorPage = new PageEntry { Slug = InternalErrorSlug };
            contentfulService.GetPageByIdAsync(internalErrorPageId)
                .Returns(Task.FromResult<PageEntry>(internalErrorPage));

            _middleware = new ServiceExceptionHandlerMiddleware(errorPagesOptions, contentfulService);
        }

        [Fact]
        public async Task Should_Get_ServerError_Redirect_On_Null_Exception()
        {
            var context = new DefaultHttpContext();

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(InternalErrorSlug, context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_ServerError_Redirect_On_ContentfulDataUnavailableException()
        {
            var exception = new ContentfulDataUnavailableException("contentful unavailable");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(InternalErrorSlug, context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_ServerError_Redirect_On_DatabaseException()
        {
            var exception = new DatabaseException("db exception");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(InternalErrorSlug, context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_ServerError_Redirect_On_InvalidEstablishmentException()
        {
            var exception = new InvalidEstablishmentException("invalid estab");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(InternalErrorSlug, context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_OrgError_Redirect_On_KeyNotFoundException_With_Organisation()
        {
            var exception = new KeyNotFoundException("organisation missing");
            var feature = new ExceptionHandlerFeature { Error = exception };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(UrlConstants.OrgErrorPage, context.Response.Headers.Values.FirstOrDefault());
        }

        [Fact]
        public async Task Should_Get_ServerError_Redirect_On_InnerException()
        {
            var inner = new ContentfulDataUnavailableException("inner contentful error");
            var outer = new Exception("outer", inner);
            var feature = new ExceptionHandlerFeature { Error = outer };

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerPathFeature>(feature);

            await _middleware.HandleExceptionAsync(context);

            Assert.NotNull(context.Response);
            Assert.Equal(InternalErrorSlug, context.Response.Headers.Values.FirstOrDefault());
        }
    }
}
