using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SignInServiceTests
{
    private readonly IEstablishmentWorkflow _establishmentWorkflow =
        Substitute.For<IEstablishmentWorkflow>();
    private readonly ISignInWorkflow _signInWorkflow = Substitute.For<ISignInWorkflow>();
    private readonly IUserWorkflow _userWorkflow = Substitute.For<IUserWorkflow>();

    private SignInService CreateServiceUnderTest() =>
        new(_establishmentWorkflow, _signInWorkflow, _userWorkflow);

    // ── ctor guards ────────────────────────────────────────────────────────────
    [Fact]
    public void Ctor_NullDeps_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SignInService(null!, _signInWorkflow, _userWorkflow)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SignInService(_establishmentWorkflow, null!, _userWorkflow)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new SignInService(_establishmentWorkflow, _signInWorkflow, null!)
        );
    }

    // ── RecordSignInAsync: user & establishment ────────────────────────────
    [Fact]
    public async Task RecordSignIn_UsesExistingUserAndEstablishment()
    {
        await _userWorkflow.Received(1).GetOrCreateUserAsync(Arg.Any<string>());
        await _establishmentWorkflow
            .Received(1)
            .GetOrCreateEstablishmentAsync(Arg.Any<EstablishmentModel>());
        await _signInWorkflow.Received(1).CreateSignInAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    // ── RecordSignInUserOnly: user only ────────
    [Fact]
    public async Task RecordSignIn_Creates_Missing_User_And_Establishment_With_Copied_Fields()
    {
        await _userWorkflow.Received(1).GetOrCreateUserAsync(Arg.Any<string>());
        await _signInWorkflow.Received(1).CreateSignInAsync(Arg.Any<int>(), Arg.Any<int>());
    }
}
