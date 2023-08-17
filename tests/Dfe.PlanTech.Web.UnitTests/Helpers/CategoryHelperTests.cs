using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.Helpers.Tests
{
    public class CategoryHelperTests
    {
        private readonly IGetSubmissionStatusesQuery _submissionStatusesQuery;
        private readonly ILogger<Category> _logger;
        private readonly CategoryHelper _categoryHelper;

        public CategoryHelperTests()
        {
            _submissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _logger = Substitute.For<ILogger<Category>>();
            _categoryHelper = new CategoryHelper(_submissionStatusesQuery, _logger);
        }

        [Fact]
        public void RetrieveSectionStatuses_SuccessfulRetrieval_ReturnsSectionStatusesAndNoError()
        {
            
            var section1 = Substitute.For<ISection>();
            var section2 = Substitute.For<ISection>();
            
            var sections = new ISection[] { section1, section2 };
            
            var expectedStatuses = new List<SectionStatuses> { new SectionStatuses(), new SectionStatuses() };
            _submissionStatusesQuery.GetSectionSubmissionStatuses(sections).Returns(expectedStatuses);
            
            var (actualStatuses, actualError) = _categoryHelper.RetrieveSectionStatuses(sections);

            Assert.NotEmpty(actualStatuses);
            Assert.False(actualError);
        }

        [Fact]
        public void RetrieveSectionStatuses_ExceptionThrown_ReturnsEmptyStatusesAndError()
        {
            var section1 = Substitute.For<ISection>();
            var section2 = Substitute.For<ISection>();
            var sections = new ISection[] { section1, section2 }; 
            _submissionStatusesQuery.GetSectionSubmissionStatuses(sections).Throws(new Exception("test"));

            var (actualStatuses, actualError) = _categoryHelper.RetrieveSectionStatuses(sections);

            Assert.Empty(actualStatuses);
            Assert.True(actualError);
            _logger.Received(1).LogError(Arg.Any<string>());
        }

    }
}
