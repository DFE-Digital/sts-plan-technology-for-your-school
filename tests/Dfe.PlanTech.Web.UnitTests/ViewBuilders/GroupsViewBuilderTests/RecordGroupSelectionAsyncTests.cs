using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RecordGroupSelectionAsyncTests : GroupsViewBuilderTestBase
{
    [Fact]
    public async Task RecordGroupSelectionAsync_WhenCalled_RecordsSelectionAgainstUserAndTrustEstablishment()
    {
        // Arrange
        _currentUserProvider.UserId.Returns(7);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(500).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RecordGroupSelectionAsync(11, "100011", "Oakfield Primary");

        // Assert
        await _establishmentService
            .Received(1)
            .RecordGroupSelection(7, 500, 11, "100011", "Oakfield Primary");
    }
}
