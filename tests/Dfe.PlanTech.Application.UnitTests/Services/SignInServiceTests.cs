using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class SignInServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        Substitute.For<IEstablishmentWorkflow>();
    private readonly ISignInWorkflow _signInWorkflow = Substitute.For<ISignInWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private SignInService CreateSystemUnderTest() =>
        new(_establishmentWorkflow, _signInWorkflow, _userWorkflow);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<SignInService>(
            _establishmentWorkflow,
            _signInWorkflow,
            _userWorkflow
        );
    }

    [Fact]
    public async Task RecordSignInAsync_WhenCalled_CreatesSignInForUserAndOrganisationEstablishment()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder().AsMultiAcademyTrust().Build();
        var expectedSignIn = new SqlSignInDtoBuilder()
            .WithUserId(42)
            .WithEstablishmentId(301)
            .Build();
        _userWorkflow
            .GetOrCreateUserAsync("dsi-user-42")
            .Returns(new SqlUserDto { Id = 42, DfeSignInRef = "dsi-user-42" });
        _establishmentWorkflow
            .GetOrCreateEstablishmentAsync(organisation)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());
        _signInWorkflow.CreateSignInAsync(42, 301).Returns(expectedSignIn);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RecordSignInAsync("dsi-user-42", organisation);

        // Assert
        Assert.Same(expectedSignIn, result);
    }

    [Fact]
    public async Task RecordSignInUserOnlyAsync_WhenCalled_CreatesSignInWithoutEstablishment()
    {
        // Arrange
        var expectedSignIn = new SqlSignInDtoBuilder()
            .WithUserId(42)
            .WithEstablishmentId(null)
            .Build();
        _userWorkflow
            .GetOrCreateUserAsync("dsi-user-42")
            .Returns(new SqlUserDto { Id = 42, DfeSignInRef = "dsi-user-42" });
        _signInWorkflow.CreateSignInAsync(42, null).Returns(expectedSignIn);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RecordSignInUserOnlyAsync("dsi-user-42");

        // Assert
        Assert.Same(expectedSignIn, result);
        await _establishmentWorkflow
            .DidNotReceive()
            .GetOrCreateEstablishmentAsync(Arg.Any<Dfe.PlanTech.Core.Models.EstablishmentModel>());
    }
}
