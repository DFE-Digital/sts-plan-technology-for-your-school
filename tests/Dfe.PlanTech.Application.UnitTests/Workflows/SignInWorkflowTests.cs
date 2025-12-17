using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SignInWorkflowTests
{
    private readonly IEstablishmentRepository _estRepo = Substitute.For<IEstablishmentRepository>();
    private readonly ISignInRepository _signInRepo = Substitute.For<ISignInRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();

    private SignInWorkflow CreateServiceUnderTest() => new(_estRepo, _signInRepo, _userRepo);

    // ── ctor guards ────────────────────────────────────────────────────────────
    [Fact]
    public void Ctor_NullDeps_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new SignInWorkflow(null!, _signInRepo, _userRepo));
        Assert.Throws<ArgumentNullException>(() => new SignInWorkflow(_estRepo, null!, _userRepo));
        Assert.Throws<ArgumentNullException>(() => new SignInWorkflow(_estRepo, _signInRepo, null!));
    }

    // ── RecordSignIn: existing user & establishment ────────────────────────────
    [Fact]
    public async Task RecordSignIn_UsesExistingUserAndEstablishment()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var dsi = "user-123";
        var name = "testName";
        var urn = "testUrn";
        var model = new EstablishmentModel { Urn = urn, Name = name };

        var user = new UserEntity
        {
            Id = 7,
            DfeSignInRef = dsi,
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
        };

        var signIn = new SignInEntity
        {
            Id = 101,
            UserId = 7,
            EstablishmentId = 10,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };

        var establishment = new EstablishmentEntity
        {
            Id = 10,
            EstablishmentRef = urn,
            OrgName = name
        };

        _userRepo.GetUserBySignInRefAsync(dsi).Returns(user);
        _estRepo.GetEstablishmentByReferenceAsync(urn).Returns(establishment);
        _signInRepo.CreateSignInAsync(7, 10).Returns(signIn);

        var dto = await serviceUnderTest.RecordSignIn(dsi, model);

        Assert.Equal(101, dto.Id);
        await _userRepo.Received(1).GetUserBySignInRefAsync(dsi);
        await _estRepo.Received(1).GetEstablishmentByReferenceAsync(urn);
        await _signInRepo.Received(1).CreateSignInAsync(7, 10);
        await _userRepo.DidNotReceive().CreateUserBySignInRefAsync(Arg.Any<string>());
        await _estRepo.DidNotReceive().CreateEstablishmentFromModelAsync(Arg.Any<EstablishmentModel>());
    }

    // ── RecordSignIn: create missing user & establishment; copy fields ────────
    [Fact]
    public async Task RecordSignIn_Creates_Missing_User_And_Establishment_With_Copied_Fields()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var dsi = "user-456";
        var name = "testName";
        var urn = "testUrn";
        var model = new EstablishmentModel
        {
            Ukprn = "UKPRN-XYZ",
            Urn = urn,
            Type = new IdWithNameModel { Name = name },
            Name = name,
            Uid = "GROUP-UID-9"
        };

        var user = new UserEntity
        {
            Id = 9,
            DfeSignInRef = dsi,
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
        };

        var signIn = new SignInEntity
        {
            Id = 202,
            UserId = 9,
            EstablishmentId = 20,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };

        _userRepo.GetUserBySignInRefAsync(dsi).Returns((UserEntity?)null);
        _userRepo.CreateUserBySignInRefAsync(dsi).Returns(user);

        _estRepo.GetEstablishmentByReferenceAsync("URN-2").Returns((EstablishmentEntity?)null);

        EstablishmentModel? captured = null;
        _estRepo.CreateEstablishmentFromModelAsync(Arg.Do<EstablishmentModel>(m => captured = m))
                .Returns(new EstablishmentEntity { Id = 20, EstablishmentRef = urn, OrgName = name });

        _signInRepo.CreateSignInAsync(9, 20).Returns(signIn);

        var dto = await serviceUnderTest.RecordSignIn(dsi, model);

        Assert.Equal(202, dto.Id);

        // Verify the *copied* model fields sent to repository
        Assert.NotNull(captured);
        Assert.Equal(model.Ukprn, captured!.Ukprn);
        Assert.Equal(model.Urn, captured.Urn);
        Assert.Equal(model.Name, captured.Name);
        Assert.Equal(model.Uid, captured.GroupUid); // Uid -> GroupUid copy
        Assert.NotNull(captured.Type);
        Assert.Equal(name, captured.Type!.Name);

        await _userRepo.Received(1).CreateUserBySignInRefAsync(dsi);
        await _estRepo.Received(1).CreateEstablishmentFromModelAsync(Arg.Any<EstablishmentModel>());
        await _signInRepo.Received(1).CreateSignInAsync(9, 20);
    }

    // ── RecordSignIn: Type null branch ────────────────────────────────────────
    [Fact]
    public async Task RecordSignIn_Copies_Type_Null_When_Source_Type_Name_Null()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var dsi = "user-789";
        var name = "testName";
        var urn = "testUrn";
        var model = new EstablishmentModel
        {
            Urn = urn,
            Name = name,
            Type = new IdWithNameModel { Name = null! }
        };

        var user = new UserEntity
        {
            Id = 11,
            DfeSignInRef = dsi,
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
        };

        var signIn = new SignInEntity
        {
            Id = 303,
            UserId = 11,
            EstablishmentId = 30,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };

        _userRepo.GetUserBySignInRefAsync(dsi).Returns((UserEntity?)null);
        _userRepo.CreateUserBySignInRefAsync(dsi).Returns(user);

        _estRepo.GetEstablishmentByReferenceAsync(urn).Returns((EstablishmentEntity?)null);

        EstablishmentModel? captured = null;
        _estRepo.CreateEstablishmentFromModelAsync(Arg.Do<EstablishmentModel>(m => captured = m))
                .Returns(new EstablishmentEntity { Id = 30, EstablishmentRef = urn, OrgName = name });

        _signInRepo.CreateSignInAsync(11, 30).Returns(signIn);

        var dto = await serviceUnderTest.RecordSignIn(dsi, model);

        Assert.Equal(303, dto.Id);
        Assert.NotNull(captured);
        Assert.Null(captured!.Type); // because source Type.Name was null
    }

    // ── RecordSignInUserOnly: existing user ───────────────────────────────────
    [Fact]
    public async Task RecordSignInUserOnly_Existing_User()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var dsi = "user-existing";
        var user = new UserEntity
        {
            Id = 55,
            DfeSignInRef = dsi,
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
        };

        var signIn = new SignInEntity
        {
            Id = 501,
            UserId = 55,
            EstablishmentId = 50,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };

        _userRepo.GetUserBySignInRefAsync(dsi).Returns(user);
        _signInRepo.CreateSignInAsync(55).Returns(signIn);

        var dto = await serviceUnderTest.RecordSignInUserOnly(dsi);

        Assert.Equal(501, dto.Id);
        await _signInRepo.Received(1).CreateSignInAsync(55);
        await _userRepo.DidNotReceive().CreateUserBySignInRefAsync(Arg.Any<string>());
    }

    // ── RecordSignInUserOnly: new user ────────────────────────────────────────
    [Fact]
    public async Task RecordSignInUserOnly_Creates_User_When_Missing()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var dsi = "user-new";
        var user = new UserEntity
        {
            Id = 77,
            DfeSignInRef = dsi,
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
        };

        var signIn = new SignInEntity
        {
            Id = 701,
            UserId = 77,
            EstablishmentId = 70,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };

        _userRepo.GetUserBySignInRefAsync(dsi).Returns((UserEntity?)null);
        _userRepo.CreateUserBySignInRefAsync(dsi).Returns(user);

        _signInRepo.CreateSignInAsync(77).Returns(signIn);

        var dto = await serviceUnderTest.RecordSignInUserOnly(dsi);

        Assert.Equal(701, dto.Id);
        await _userRepo.Received(1).CreateUserBySignInRefAsync(dsi);
        await _signInRepo.Received(1).CreateSignInAsync(77);
    }
}

