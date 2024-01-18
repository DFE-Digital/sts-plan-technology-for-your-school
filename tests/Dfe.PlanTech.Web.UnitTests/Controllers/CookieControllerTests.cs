using AutoMapper;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Cookie.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CookieControllerTests
    {
        private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();
        private readonly ILogger<GetPageQuery> _getPageLogger = Substitute.For<ILogger<GetPageQuery>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IGetPageQuery _getPageFromDbQuery;

        private readonly Page[] _pages = new Page[]
        {
            new Page()
            {
                Slug = "cookies",
                Title = new Title() { Text = "Cookies" },
                Content = new List<ContentComponent> { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Analytical Cookies" }}
            },
        };

        public static CookiesController CreateStrut()
        {
            ILogger<CookiesController> loggerSubstitute = Substitute.For<ILogger<CookiesController>>();
            ICookieService cookiesSubstitute = Substitute.For<ICookieService>();

            return new CookiesController(loggerSubstitute, cookiesSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };
        }

        public CookieControllerTests()
        {
            _getPageFromDbQuery = Substitute.For<GetPageFromDbQuery>(_db, new NullLogger<GetPageFromDbQuery>(), _mapper, Array.Empty<IGetPageChildrenQuery>());
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void HideBanner_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;

            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = strut.HideBanner() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void Accept_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;
            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = strut.Accept() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        [Theory]
        [InlineData("https://localhost:8080/self-assessment")]
        [InlineData("https://www.dfe.gov.uk/self-assessment")]
        public void Reject_Redirects_BackToPlaceOfOrigin(string url)
        {
            //Arrange
            var strut = CreateStrut();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = url;
            strut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            //Act
            var result = strut.Reject() as RedirectResult;

            //Assert
            Assert.IsType<RedirectResult>(result);
            Assert.Equal(url, result.Url);
        }

        private static IRequestCookieCollection SubstituteRequestCookieCollection(string key, string value)
        {
            var requestFeature = new HttpRequestFeature();
            var featureCollection = new FeatureCollection();

            requestFeature.Headers = new HeaderDictionary();
            requestFeature.Headers.Add(HeaderNames.Cookie, new StringValues(key + "=" + value));

            featureCollection.Set<IHttpRequestFeature>(requestFeature);

            var cookiesFeature = new RequestCookiesFeature(featureCollection);

            return cookiesFeature.Cookies;
        }


        [Fact]
        public async Task CookiesPageDisplays()
        {
            IQuestionnaireCacher questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();
            IContentRepository contentRepositorySubstitute = SetupRepositorySubstitute();
            GetPageQuery _getPageQuerySubstitute = Substitute.For<GetPageQuery>(_getPageFromDbQuery, _getPageLogger, questionnaireCacherSubstitute, contentRepositorySubstitute);

            CookiesController cookiesController = CreateStrut();
            var result = await cookiesController.GetCookiesPage(_getPageQuerySubstitute);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("Cookies", viewResult.ViewName);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void settingCookiePreferenceBasedOnInputRedirectsToCookiePage(string userPreference)
        {
            CookiesController cookiesController = CreateStrut();

            var tempDataSubstitute = Substitute.For<ITempDataDictionary>();
            var cookiesSubstitute = Substitute.For<ICookieService>();

            cookiesSubstitute.SetPreference(Arg.Any<bool>());

            cookiesController.TempData = tempDataSubstitute;

            var result = cookiesController.CookiePreference(userPreference);

            tempDataSubstitute.Received(1)["UserPreferenceRecorded"] = true;

            Assert.IsType<RedirectToActionResult>(result);

            var res = result as RedirectToActionResult;

            if (res != null)
            {
                Assert.True(res.ActionName == "GetByRoute");
                Assert.True(res.ControllerName == "Pages");
            }
        }

        [Fact]
        public void settingCookiePreferenceBasedOnInputAsNullThrowsException()
        {
            CookiesController cookiesController = CreateStrut();

            var tempDataSubstitute = Substitute.For<ITempDataDictionary>();
            var cookiesSubstitute = Substitute.For<ICookieService>();

            cookiesSubstitute.SetPreference(Arg.Any<bool>());

            cookiesController.TempData = tempDataSubstitute;

            var result = Assert.Throws<ArgumentException>(() => cookiesController.CookiePreference(string.Empty));
            Assert.Contains("Can't convert preference", result.Message);
        }

        private IContentRepository SetupRepositorySubstitute()
        {
            var repositorySubstitute = Substitute.For<IContentRepository>();
            repositorySubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns((CallInfo) =>
            {
                IGetEntitiesOptions options = (IGetEntitiesOptions)CallInfo[0];
                if (options?.Queries != null)
                {
                    foreach (var query in options.Queries)
                    {
                        if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                        {
                            return _pages.Where(page => page.Slug == equalsQuery.Value);
                        }
                    }
                }
                return Array.Empty<Page>();
            });
            return repositorySubstitute;
        }
    }
}
