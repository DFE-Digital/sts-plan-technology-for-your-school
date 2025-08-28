using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class UserWorkflowTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();

    private UserWorkflow CreateServiceUnderTest() => new UserWorkflow(_repo);

    // --- ctor guard ---
    [Fact]
    public void Ctor_NullRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UserWorkflow(null!));
    }

    // --- GetUserBySignInRefAsync ---

    [Fact]
    public async Task GetUserBySignInRefAsync_Returns_Null_When_Not_Found()
    {
        var sut = CreateServiceUnderTest();
        _repo.GetUserBySignInRefAsync("dsi-123").Returns((UserEntity?)null);

        var result = await sut.GetUserBySignInRefAsync("dsi-123");

        Assert.Null(result);
        await _repo.Received(1).GetUserBySignInRefAsync("dsi-123");
    }

    [Fact]
    public async Task GetUserBySignInRefAsync_Returns_Mapped_Dto_When_Found()
    {
        var sut = CreateServiceUnderTest();
        var entity = new UserEntity { Id = 42, DfeSignInRef = "dsi-abc" };
        _repo.GetUserBySignInRefAsync("dsi-abc").Returns(entity);

        var dto = await sut.GetUserBySignInRefAsync("dsi-abc");

        Assert.NotNull(dto);
        Assert.Equal(42, dto!.Id);
        Assert.Equal("dsi-abc", dto.DfeSignInRef);
        await _repo.Received(1).GetUserBySignInRefAsync("dsi-abc");
    }
}
