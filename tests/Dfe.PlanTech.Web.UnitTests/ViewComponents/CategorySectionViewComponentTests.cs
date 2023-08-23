using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class CategorySectionViewComponentTests
    {
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly CategorySectionViewComponent _categorySectionViewComponent;

        private ICategory _category;

        public CategorySectionViewComponentTests()
        {
            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
            var viewContext = new ViewContext();
            viewContext.TempData = tempData;
            var viewComponentContext = new ViewComponentContext();
            viewComponentContext.ViewContext = viewContext;

            _categorySectionViewComponent = new CategorySectionViewComponent(Substitute.For<ILogger<CategorySectionViewComponent>>(), _getSubmissionStatusesQuery);
            _categorySectionViewComponent.ViewComponentContext = viewComponentContext;

            _category = new Category()
            {
                Completed = 1,
                Sections = new Section[]
                {
                    new Section()
                    {
                        Sys = new Sys() { Id = "Section1" },
                        Name = "Test Section 1",
                        InterstitialPage = new Domain.Content.Models.Page()
                        {
                            Slug = "section-1",
                        }
                    }
                }
            };
        }

        [Fact]
        public void Returns_CategorySectionInfo_If_Slug_Exists_And_SectionIsCompleted()
        {
            _category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 1,
            });

            _getSubmissionStatusesQuery.GetCategoryWithCompletedSectionStatuses(_category).Returns(_category);

            var result = _categorySectionViewComponent.Invoke(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(1, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto as IEnumerable<CategorySectionDto>;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("DarkBlue", categorySectionDto.TagColour);
            Assert.Equal("COMPLETE", categorySectionDto.TagText);
            Assert.Null(categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public void Returns_CategorySelectionInfo_If_Slug_Exists_And_SectionIsNotCompleted()
        {
            // TODO: Implement Test
        }

        [Fact]
        public void Returns_NullSlug_And_ErrorMessage_In_CategorySectionInfo_If_SlugDoesNotExist()
        {
            // TODO: Implement Test
        }
    }
}