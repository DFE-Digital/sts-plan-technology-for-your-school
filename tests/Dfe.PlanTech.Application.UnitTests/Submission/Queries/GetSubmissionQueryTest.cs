using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetSubmissionQueryTest
    {
        private IPlanTechDbContext _planTechDbContextMock;

        private readonly GetSubmissionQuery _getSubmissionQuery;

        public GetSubmissionQueryTest()
        {
            _planTechDbContextMock = Substitute.For<IPlanTechDbContext>();

            _getSubmissionQuery = new GetSubmissionQuery(_planTechDbContextMock);
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

            _planTechDbContextMock.GetSubmissions.Returns(submissionList.AsQueryable());
            _planTechDbContextMock.FirstOrDefaultAsync(submissionList.AsQueryable()).Returns(submissionList[0]);

            var result = await _getSubmissionQuery.GetSubmissionBy(16, "SectionId");

            Assert.IsType<Domain.Submissions.Models.Submission>(result);
            Assert.Equal(submissionList[0], result);
        }
    }
}