using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class AccessibilityControllerTests
    {
        private readonly Page[] _pages = new Page[]
        {
            new Page()
            {
                Slug = "accessibility",
                Title = new Title() { Text = "Accessibility" },
                Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Accessibility" }}
            },
        };


        public static AccessibilityController CreateStrut()
        {
            ILogger<AccessibilityController> loggerSubstitute = Substitute.For<ILogger<AccessibilityController>>();
            
            return new AccessibilityController(loggerSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };
        }

        [Fact]
         public async Task AccessibilityPageDisplays()
         {
            IQuestionnaireCacher questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();
            IContentRepository contentRepositorySubstitute = SetupRepositorySubstitute();
            GetPageQuery _getPageQuerySubstitute = Substitute.For<GetPageQuery>(questionnaireCacherSubstitute, contentRepositorySubstitute);

            AccessibilityController accessibilityController = CreateStrut();
            var result = await accessibilityController.GetAccessibilityPage(_getPageQuerySubstitute);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("Accessibility", viewResult.ViewName);
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
