using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetSubmissionQueryTest
    {
        private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;

        private readonly GetSubmissionQuery _getSubmissionQuery;

        public GetSubmissionQueryTest()
        {
            _planTechDbContextMock = new Mock<IPlanTechDbContext>();

            _getSubmissionQuery = new GetSubmissionQuery(_planTechDbContextMock.Object);
        }

        [Fact]
        public async Task GetSubmissionBy_Returns_Submission()
        {
            List<Domain.Submissions.Models.Submission> submissionList = new List<Domain.Submissions.Models.Submission>()
            {
                new Domain.Submissions.Models.Submission()
                {
                    Id = 1,
                    EstablishmentId = 16,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

            _planTechDbContextMock.Setup(m => m.GetSubmissions).Returns(submissionList.AsQueryable());
            _planTechDbContextMock.Setup(m => m.FirstOrDefaultAsync(submissionList.AsQueryable())).ReturnsAsync(submissionList[0]);

            var result = await _getSubmissionQuery.GetSubmissionBy(16, "SectionId");

            Assert.IsType<Domain.Submissions.Models.Submission>(result);
            Assert.Equal(submissionList[0], result);
        }
    }
}