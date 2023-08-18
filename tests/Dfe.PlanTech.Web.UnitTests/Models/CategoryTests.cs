using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models;

public class CategoryTests
{
    private readonly IGetSubmissionStatusesQuery _submissionStatusesQuery;
    private readonly ILogger<Category> _logger;
    private readonly Category _category;

    public CategoryTests()
    {
        _submissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
        _logger = Substitute.For<ILogger<Category>>();
        _category = new Category(_logger, _submissionStatusesQuery);
    }

    [Fact]
    public void RetrieveSectionStatuses_SuccessfulRetrieval_ReturnsSectionStatusesAndNoError()
    {
        var section1 = Substitute.For<ISection>();
        var section2 = Substitute.For<ISection>();

        var sections = new ISection[] { section1, section2 };

        var expectedStatuses = new List<SectionStatuses> { new SectionStatuses(), new SectionStatuses() };
        _submissionStatusesQuery.GetSectionSubmissionStatuses(sections).Returns(expectedStatuses);

        _category.RetrieveSectionStatuses();

        _logger.Received(0);
    }

    [Fact]
    public void RetrieveSectionStatuses_ExceptionThrown_ReturnsEmptyStatusesAndError()
    {
        var section1 = Substitute.For<ISection>();
        var section2 = Substitute.For<ISection>();
        var sections = new ISection[] { section1, section2 };
        _submissionStatusesQuery.GetSectionSubmissionStatuses(sections).Throws(new Exception("test"));

        _category.RetrieveSectionStatuses();
        ;
        _logger.Received(1);
    }
}