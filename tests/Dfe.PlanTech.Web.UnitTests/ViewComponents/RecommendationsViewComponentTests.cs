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
        private ICategory category;

        public RecommendationsViewComponentTests()
        {
            _recommendationsComponent = new RecommendationsViewComponent(Substitute.For<ILogger<RecommendationsViewComponent>>());

            category = new Category(null, null)
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
                },
            };
        }



        [Fact]
        public void Returns_RecommendationInfo_If_It_Exists_ForMaturity()
        {
            var result = _recommendationsComponent.Invoke(category) as ViewViewComponentResult;

            category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 1,
                Maturity = "High"
            });

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as IEnumerable<RecommendationsViewComponentViewModel>;
            Assert.NotNull(unboxed);

            unboxed = unboxed.ToList();
            Assert.NotEmpty(unboxed);

            Assert.Equal(category.Sections[0].Recommendations[0].Page.Slug, unboxed.First().RecommendationSlug);
            Assert.Equal(category.Sections[0].Recommendations[0].DisplayName, unboxed.First().RecommendationDisplayName);
            Assert.Null(unboxed.First().NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public void Returns_NullRecommendationInfo_If_No_RecommendationPage_Exists_ForMaturity()
        {
            category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 1,
                Maturity = "Low"
            });

            var result = _recommendationsComponent.Invoke(category) as ViewViewComponentResult;

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
            category.SectionStatuses.Add(new Domain.Submissions.Models.SectionStatuses()
            {
                SectionId = "Section1",
                Completed = 0,
                Maturity = null
            });

            var result = _recommendationsComponent.Invoke(category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as IEnumerable<RecommendationsViewComponentViewModel>;
            Assert.NotNull(unboxed);

            unboxed = unboxed.ToList();
            Assert.Empty(unboxed);
        }

        [Fact]
        public void Returns_Null_If_Category_IsNot_Completed()
        {
            category.Completed = 0;

            var result = _recommendationsComponent.Invoke(category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.Null(model);
        }
    }
}