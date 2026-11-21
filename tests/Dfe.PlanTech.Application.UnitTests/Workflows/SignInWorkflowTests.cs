using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SignInWorkflowTests
{
    private readonly ISignInRepository _signInRepository = Substitute.For<ISignInRepository>();

    private SignInWorkflow CreateSystemUnderTest() => new(_signInRepository);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<SignInWorkflow>(
            _signInRepository
        );
    }

    [Fact]
    public async Task CreateSignInAsync_WhenEstablishmentIsGiven_ReturnsMappedSignIn()
    {
        // Arrange
        _signInRepository
            .CreateSignInAsync(42, 301)
            .Returns(
                new SignInEntity
                {
                    Id = 9001,
                    UserId = 42,
                    EstablishmentId = 301,
                    User = new UserEntityBuilder().WithDfeSignInRef("dsi-user-42").Build(),
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.CreateSignInAsync(42, 301);

        // Assert
        Assert.Equal(9001, result.Id);
        Assert.Equal(42, result.UserId);
        Assert.Equal(301, result.EstablishmentId);
        Assert.Equal("dsi-user-42", result.User.DfeSignInRef);
    }

    [Fact]
    public async Task CreateSignInAsync_WhenNoEstablishmentIsGiven_ReturnsSignInWithoutEstablishment()
    {
        // Arrange
        _signInRepository
            .CreateSignInAsync(42, null)
            .Returns(
                new SignInEntity
                {
                    Id = 9002,
                    UserId = 42,
                    EstablishmentId = null,
                    User = new UserEntityBuilder().Build(),
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.CreateSignInAsync(42);

        // Assert
        Assert.Equal(9002, result.Id);
        Assert.Null(result.EstablishmentId);
    }
}
