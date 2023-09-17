using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries
{
    public class GetSubmissionQueryTest
    {
        private IPlanTechDbContext _planTechDbContextSubstitute;

        private readonly GetSubmissionQuery _getSubmissionQuery;

        public GetSubmissionQueryTest()
        {
            _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();

            _getSubmissionQuery = new GetSubmissionQuery(_planTechDbContextSubstitute);
        }

        [Fact]
        public async Task GetSubmissionBy_Returns_Submission()
        {
            List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 16,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            var query = Arg.Any<IQueryable<Submission>>();
            _planTechDbContextSubstitute.FirstOrDefaultAsync(query).Returns(submissionList[0]);

            var result = await _getSubmissionQuery.GetSubmissionBy(16, "SectionId");

            Assert.IsType<Submission>(result);
            Assert.Equal(submissionList[0], result);
        }
    }
}