using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries
{
    public class GetSubmissionStatusesQueryTests
    {
        private readonly IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();
        private readonly IUser user = Substitute.For<IUser>();

        private readonly List<SectionStatusDto> SectionStatuses = new() {
            new SectionStatusDto { Completed = 1, SectionId = "1", Maturity = "Low", DateCreated = DateTime.UtcNow },
            new SectionStatusDto { Completed = 1, SectionId = "2", Maturity = "High", DateCreated = DateTime.UtcNow },
            new SectionStatusDto { Completed = 0, SectionId = "3", DateCreated = DateTime.UtcNow },
            new SectionStatusDto { Completed = 0, SectionId = "4",  DateCreated = DateTime.UtcNow },
        };

        private GetSubmissionStatusesQuery CreateStrut() => new GetSubmissionStatusesQuery(Db, user);

        public GetSubmissionStatusesQueryTests()
        {
            Db.GetSectionStatuses(Arg.Any<string>(), Arg.Any<int>())
            .Returns((callinfo) =>
            {
                var sectionIds = callinfo.ArgAt<string>(0).Split(",");

                return SectionStatuses.Where(sectionStatus => sectionIds.Any(id => id == sectionStatus.SectionId)).AsQueryable();
            });
        }

        [Fact]
        public void GetSectionSubmissionStatuses_ReturnsListOfStatuses()
        {
            var sections = new Section[2] { new() { Sys = new SystemDetails { Id = "1" } }, new() { Sys = new SystemDetails { Id = "3" } } };

            var result = CreateStrut().GetSectionSubmissionStatuses(sections);

            Assert.Equal(result.Count, sections.Length);

            foreach (var section in sections)
            {
                var matchingSectionStatus = result.FirstOrDefault(sectionStatus => sectionStatus.SectionId == section.Sys.Id);

                Assert.NotNull(matchingSectionStatus);
            }
        }
    }
}
