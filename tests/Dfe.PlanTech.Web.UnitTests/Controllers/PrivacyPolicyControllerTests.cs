using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PrivacyPolicyControllerTests
    {
        private readonly Page[] _pages = new Page[]
        {
            new Page()
            {
                Slug = "privacy-policy",
                Title = new Title() { Text = "Privacy Policy" },
                Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Analytical Cookies" }}
            },
        };

        public static PrivacyPolicyController CreateStrut()
        {
            ILogger<PrivacyPolicyController> loggerSubstitute = Substitute.For<ILogger<PrivacyPolicyController>>();

            return new PrivacyPolicyController(loggerSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };
        }

        [Fact]
        public async Task PrivacyPolicyPageDisplays()
        {
            IQuestionnaireCacher questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();
            IContentRepository contentRepositorySubstitute = SetupRepositorySubstitute();
            GetPageQuery _getPageQuerySubstitute = Substitute.For<GetPageQuery>(questionnaireCacherSubstitute, contentRepositorySubstitute);

            var controller = CreateStrut();
            var result = await controller.GetPrivacyPage(_getPageQuerySubstitute);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("Page", viewResult.ViewName);
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
