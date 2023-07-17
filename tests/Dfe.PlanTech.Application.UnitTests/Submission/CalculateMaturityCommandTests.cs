using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Application.UnitTests.Submission
{
    public class CalculateMaturityCommandTests
    {
        public Mock<IPlanTechDbContext> _dbMock = new Mock<IPlanTechDbContext>();

        private CalculateMaturityCommand CreateStrut()
        {
            return new CalculateMaturityCommand(_dbMock.Object);
        }

        [Fact]
        public async Task CalculateMaturityReturnsEffectedRows_LargerThanOne()
        {
            _dbMock.Setup(x => x.CallStoredProcedureWithReturnInt(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()))
                .ReturnsAsync(2);

            var result = await CreateStrut().CalculateMaturityAsync(It.IsAny<int>());

            Assert.True(result > 1);
        }

        [Fact]
        public async Task CalculateMaturityReturnsEffectedRows_LessThanOne()
        {
            _dbMock.Setup(x => x.CallStoredProcedureWithReturnInt(It.IsAny<string>(), It.IsAny<List<SqlParameter>>()))
                            .ReturnsAsync(0);

            var result = await CreateStrut().CalculateMaturityAsync(It.IsAny<int>());

            Assert.True(result < 1);
        }
    }
}
