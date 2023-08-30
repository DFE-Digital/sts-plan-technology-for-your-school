using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class RecommendationsViewComponentTests
    {
        private readonly RecommendationsViewComponent _recommendationsComponent;
        private ICategory _category;
        private IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private ILogger<Category> _loggerCategory;

        public RecommendationsViewComponentTests()
        {

            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _loggerCategory = Substitute.For<ILogger<Category>>();

            _recommendationsComponent = new RecommendationsViewComponent(Substitute.For<ILogger<RecommendationsViewComponent>>());

            _category = new Category(_loggerCategory, _getSubmissionStatusesQuery)
            {
                Completed = 1,
                Sections = new Section[]
                {
                    new Section()
                    {
                        Sys = new Sys() { Id = "Section1" },
                        Name = "Test Section 1",
                        Recommendations = new RecommendationPage[]
                        {
                            new RecommendationPage()
                            {
                                InternalName = "High-Maturity-Recommendation-Page-InternalName",
                                DisplayName = "High-Maturity-Recommendation-Page-DisplayName",
                                Maturity = Domain.Questionnaire.Enums.Maturity.High,
                                Page = new Domain.Content.Models.Page() { Slug = "High-Maturity-Recommendation-Page-Slug" }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void Returns_RecommendationInfo_If_It_Exists_ForMaturity()
        {
            _category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 1,
                Maturity = "High"
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns(_category.SectionStatuses);

            var result = _recommendationsComponent.Invoke(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as IEnumerable<RecommendationsViewComponentViewModel>;
            Assert.NotNull(unboxed);

            unboxed = unboxed.ToList();
            Assert.NotEmpty(unboxed);

            Assert.Equal(_category.Sections[0].Recommendations[0].Page.Slug, unboxed.First().RecommendationSlug);
            Assert.Equal(_category.Sections[0].Recommendations[0].DisplayName, unboxed.First().RecommendationDisplayName);
            Assert.Null(unboxed.First().NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public void Returns_NullRecommendationInfo_If_No_RecommendationPage_Exists_ForMaturity()
        {
            _category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 1,
                Maturity = "Low"
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns(_category.SectionStatuses);

            var result = _recommendationsComponent.Invoke(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as IEnumerable<RecommendationsViewComponentViewModel>;
            Assert.NotNull(unboxed);

            unboxed = unboxed.ToList();
            Assert.NotEmpty(unboxed);

            Assert.Null(unboxed.First().RecommendationSlug);
            Assert.Null(unboxed.First().RecommendationDisplayName);
            Assert.NotNull(unboxed.First().NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public void DoesNotReturn_RecommendationInfo_If_Section_IsNot_Completed()
        {
            _category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 0,
                Maturity = null
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns(_category.SectionStatuses);

            var result = _recommendationsComponent.Invoke(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.Null(model);
        }

        [Fact]
        public void Returns_Null_If_Category_IsNot_Completed()
        {
            _category.Completed = 0;

            var result = _recommendationsComponent.Invoke(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.Null(model);
        }
    }
}