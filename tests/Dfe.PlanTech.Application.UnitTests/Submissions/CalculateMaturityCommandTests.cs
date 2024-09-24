using System.Data.SqlTypes;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Data.SqlClient;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions;
public class CalculateMaturityCommandTests
{
    public IPlanTechDbContext _dbSubstitute = Substitute.For<IPlanTechDbContext>();
    private List<SqlParameter>? _sqlParams = null;

    public CalculateMaturityCommandTests()
    {
        _dbSubstitute.CallStoredProcedureWithReturnInt(DatabaseConstants.CalculateMaturitySproc, Arg.Any<List<SqlParameter>>())
        .Returns((callinfo) =>
        {
            var sqlParams = callinfo.ArgAt<List<SqlParameter>>(1);

            _sqlParams = sqlParams;

            var submissionIdParam = _sqlParams.FirstOrDefault(sqlParam => sqlParam.ParameterName == DatabaseConstants.CalculateMaturitySprocParam);

            if (submissionIdParam != null && submissionIdParam.Value is int i && i > 0)
            {
                return i;
            }

            throw new SqlTypeException("Invalid submission Id");
        });
    }
    private CalculateMaturityCommand CreateStrut() => new(_dbSubstitute);

    [Fact]
    public async Task CalculateMaturityReturnsEffectedRows_LargerThanOne()
    {
        var submissionId = 2;

        var result = await CreateStrut().CalculateMaturityAsync(submissionId);

        Assert.Equal(submissionId, result);
    }

    [Fact]
    public async Task CalculateMaturityReturnsEffectedRows_LessThanOne()
    {
        await Assert.ThrowsAsync<SqlTypeException>(() => CreateStrut().CalculateMaturityAsync(0));
    }
}
