using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class GroupServiceTests
{
    private readonly IGroupWorkflow _groupWorkflow =
        Substitute.For<IGroupWorkflow>();

    private GroupService CreateServiceUnderTest() =>
        new GroupService(_groupWorkflow);

    [Fact]
    public async Task GetGroupCompletedSubmissionsBySections_Calls_Workflow_And_Returns_Result()
    {
        // Arrange
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3 };

        var expected = new List<SqlSubmissionDto>
        {
            new() { Id = 100, EstablishmentId = 1 },
            new() { Id = 200, EstablishmentId = 2 },
        };

        _groupWorkflow
            .GetGroupCompletedSubmissions(establishmentIds)
            .Returns(expected);

        // Act
        var result = await sut.GetGroupCompletedSubmissionsBySections(establishmentIds);

        // Assert
        Assert.Same(expected, result);

        await _groupWorkflow
            .Received(1)
            .GetGroupCompletedSubmissions(
                Arg.Is<int[]>(ids =>
                    ids.SequenceEqual(establishmentIds)
                )
            );
    }

    [Fact]
    public void Constructor_Throws_ArgumentNullException_When_GroupWorkflow_Is_Null()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => new GroupService(null!));

        // Assert
        Assert.Equal("groupWorkflow", exception.ParamName);
    }
}
