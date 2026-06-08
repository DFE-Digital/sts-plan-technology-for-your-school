using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using NSubstitute;
using System.Threading.Tasks;

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
    public async Task GetGroupSubmissionStatusesBySection_Calls_Workflow_And_Returns_Result()
    {
        var sut = CreateServiceUnderTest();

        var establishmentIds = new[] { 1, 2, 3 };
        var establishmentRefs = new[] { "testRef1", "testRef2", "testRef3" };
        var sectionId = "sec1";

        var expected = new List<SubmissionInformationModel>
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentName = "Establishment One",
                SectionId = sectionId,
                SubmissionId = 100,
                Status = SubmissionStatus.CompleteReviewed
            },
            new()
            {
                EstablishmentId = 2,
                EstablishmentName = "Establishment Two",
                SectionId = sectionId,
                SubmissionId = 200,
                Status = SubmissionStatus.InProgress
            },
            new()
            {
                EstablishmentId = 3,
                EstablishmentName = "Establishment Three",
                SectionId = sectionId,
                Status = SubmissionStatus.NotStarted
            }
        };

        _groupWorkflow
            .GetGroupSubmissionInformationForSection(establishmentRefs, sectionId)
            .Returns(expected);

        var result = await sut.GetGroupSubmissionInformationForSection(establishmentRefs, sectionId);

        Assert.Same(expected, result);

        await _groupWorkflow
            .Received(1)
            .GetGroupSubmissionInformationForSection(
                Arg.Is<string[]>(refs => refs.SequenceEqual(establishmentRefs)),
                Arg.Is<string>(section => section.Equals(sectionId))
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
