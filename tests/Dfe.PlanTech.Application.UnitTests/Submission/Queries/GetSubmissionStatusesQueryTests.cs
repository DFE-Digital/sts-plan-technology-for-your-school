using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetSubmissionStatusesQueryTests
    {
        private Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();

        [Fact]
        public void GetSectionSubmissionStatuses_ReturnsListOfStatuses()
        {
            var expectedStatuses = new List<SectionStatuses>() { new SectionStatuses { Completed = 1, SectionId = "1" } }.AsQueryable();
            var sections = new Section[1] { new Section { Sys = new Sys { Id = "1"} } };
            mockDb.Setup(x => x.GetSectionStatuses(It.IsAny<string>())).Returns(expectedStatuses);

            var result = CreateStrut().GetSectionSubmissionStatuses(sections);

            Assert.Equal(result.Count, expectedStatuses.ToList().Count);
        }

        private GetSubmissionStatusesQuery CreateStrut()
        {
            return new GetSubmissionStatusesQuery(mockDb.Object);
        }
    }
}
