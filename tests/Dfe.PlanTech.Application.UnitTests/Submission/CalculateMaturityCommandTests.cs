using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Commands;
using Microsoft.Data.SqlClient;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submission
{
    public class CalculateMaturityCommandTests
    {
        public IPlanTechDbContext _dbSubstitute = Substitute.For<IPlanTechDbContext>();

        private CalculateMaturityCommand CreateStrut()
        {
            return new CalculateMaturityCommand(_dbSubstitute);
        }

        [Fact]
        public async Task CalculateMaturityReturnsEffectedRows_LargerThanOne()
        {
            _dbSubstitute.CallStoredProcedureWithReturnInt(Arg.Any<string>(), Arg.Any<List<SqlParameter>>())
                .Returns(Task.FromResult(2));

            var result = await CreateStrut().CalculateMaturityAsync(2);

            Assert.True(result > 1);
        }

        [Fact]
        public async Task CalculateMaturityReturnsEffectedRows_LessThanOne()
        {
            _dbSubstitute.CallStoredProcedureWithReturnInt(Arg.Any<string>(), Arg.Any<List<SqlParameter>>())
                .Returns(Task.FromResult(0));

            var result = await CreateStrut().CalculateMaturityAsync(0);

            Assert.True(result < 1);
        }
    }
}
