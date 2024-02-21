using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class CategorySectionViewComponentTests
    {
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly CategorySectionViewComponent _categorySectionViewComponent;

        private Category _category;
        private readonly ILogger<CategorySectionViewComponent> _loggerCategory;

        public CategorySectionViewComponentTests()
        {
            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _loggerCategory = Substitute.For<ILogger<CategorySectionViewComponent>>();

            var viewContext = new ViewContext();

            var viewComponentContext = new ViewComponentContext
            {
                ViewContext = viewContext
            };

            _categorySectionViewComponent = new CategorySectionViewComponent(_loggerCategory, _getSubmissionStatusesQuery)
            {
                ViewComponentContext = viewComponentContext
            };

            _category = new Category()
            {
                Completed = 1,
                Sys = new()
                {
                    Id = "Category-Test-Id"
                },
                Sections = new(){
                {
                    new ()
                    {
                        Sys = new SystemDetails() { Id = "Section1" },
                        Name = "Test Section 1",
                        InterstitialPage = new Page()
                        {
                            Slug = "section-1",
                        }
                    }
                }
            }
            };
        }

        [Fact]
        public async Task Returns_CategorySectionInfo_If_Slug_Exists_And_SectionIsCompleted()
        {
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = 1,
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(1, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("blue", categorySectionDto.TagColour);
            Assert.Equal("COMPLETE", categorySectionDto.TagText);
            Assert.Null(categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public async Task Returns_CategorySelectionInfo_If_Slug_Exists_And_SectionIsNotCompleted()
        {
            _category.Completed = 0;

            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = 0,
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(0, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("light-blue", categorySectionDto.TagColour);
            Assert.Equal("IN PROGRESS", categorySectionDto.TagText);
            Assert.Null(categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public async Task Returns_CategorySelectionInfo_If_Section_IsNotStarted()
        {
            _category.Completed = 0;

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(0, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("grey", categorySectionDto.TagColour);
            Assert.Equal("NOT STARTED", categorySectionDto.TagText);
            Assert.Null(categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public async Task Returns_NullSlug_And_ErrorMessage_In_CategorySectionInfo_If_SlugDoesNotExist()
        {
            _category.Sections[0] = new Section()
            {
                Sys = new SystemDetails() { Id = "Section1" },
                Name = "Test Section 1",
                InterstitialPage = new Page()
                {
                    Slug = null!,
                }
            };

            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = 1,
            });

            _ = _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(1, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Null(categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Null(categorySectionDto.TagColour);
            Assert.Null(categorySectionDto.TagText);
            Assert.NotNull(categorySectionDto.NoSlugForSubtopicErrorMessage);
            Assert.Equal("Test Section 1 unavailable", categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public async Task Returns_ProgressRetrievalError_When_ProgressCanNotBeRetrieved()
        {
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(Arg.Any<IEnumerable<ISectionComponent>>())
                                        .ThrowsAsync(new Exception("Error occurred fection sections"));

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);
            Assert.Equal("Unable to retrieve progress, please refresh your browser.", unboxed.ProgressRetrievalErrorMessage);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("red", categorySectionDto.TagColour);
            Assert.Equal("UNABLE TO RETRIEVE STATUS", categorySectionDto.TagText);
            Assert.Null(categorySectionDto.NoSlugForSubtopicErrorMessage);
        }

        [Fact]
        public async Task Returns_NoSectionsErrorRedirectUrl_If_SectionsAreNull()
        {
            _category = new Category()
            {
                Completed = 0,
                Sections = null!,
                Sys = new()
                {
                    Id = "missing-sections-category"
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);
            Assert.Equal("ServiceUnavailable", unboxed.NoSectionsErrorRedirectUrl);
            Assert.Equal(0, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.Null(categorySectionDtoList);
        }

        [Fact]
        public async Task Returns_NoSectionsErrorRedirectUrl_If_SectionsAreEmpty()
        {
            _category = new Category()
            {
                Completed = 0,
                Sections = new(),
                Sys = new()
                {
                    Id = "empty-sections-category"
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sections).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);
            Assert.Equal("ServiceUnavailable", unboxed.NoSectionsErrorRedirectUrl);
            Assert.Equal(0, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.Null(categorySectionDtoList);
        }
    }
}