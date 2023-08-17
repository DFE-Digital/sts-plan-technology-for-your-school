using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ReturnsExtensions;
using System.Security.Claims;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class UserHelperTests
{
    private readonly UserHelper _userHelper;
    private IHttpContextAccessor _httpContextAccessorMock;
    private IPlanTechDbContext _planTechDbContextMock;
    private ICreateEstablishmentCommand _createEstablishmentCommandMock;

    public UserHelperTests()
    {
        _httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        _planTechDbContextMock = Substitute.For<IPlanTechDbContext>();
        _createEstablishmentCommandMock = Substitute.For<ICreateEstablishmentCommand>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"ukprn\" : \"131\",\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
            }, "mock"));

        _httpContextAccessorMock.HttpContext.Returns(new DefaultHttpContext() { User = user });

        _userHelper = new UserHelper(_httpContextAccessorMock, _planTechDbContextMock, _createEstablishmentCommandMock);
    }

    [Fact]
    public async Task GetCurrentUserId_Returns_Correct_Id_When_UserExists_InDatabase()
    {
        _planTechDbContextMock.GetUserBy(userModel => userModel.DfeSignInRef == "1").Returns(new User() { Id = 1 });

        var result = await _userHelper.GetCurrentUserId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetEstablishmentId_Returns_Correct_Id_When_Establishment_Exists_In_DB()
    {
        _planTechDbContextMock.GetEstablishmentBy(establishment => establishment.EstablishmentRef == "131").Returns(Task.FromResult(new Establishment() { Id = 1 }));

        var result = await _userHelper.GetEstablishmentId();

        await _createEstablishmentCommandMock.Received(0).CreateEstablishment(Arg.Any<EstablishmentDto>());

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }


    [Fact]
    public async Task GetEstablishmentId_Returns_Correct_Id_When_Establishment_Does_Not_Exists_In_DB()
    {
        _planTechDbContextMock.GetEstablishmentBy(establishment => establishment.EstablishmentRef == "131")
            .ReturnsNull()
            .Returns(new Establishment() { Id = 17 });

        var result = await _userHelper.GetEstablishmentId();

        await _createEstablishmentCommandMock.Received(0).CreateEstablishment(Arg.Any<EstablishmentDto>());

        Assert.IsType<int>(result);

        Assert.Equal(17, result);
    }

    [Fact]
    public async Task GetEstablishmentId_Returns_1_When_Reference_Is_Not_Present()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
        }, "mock"));

        _httpContextAccessorMock.HttpContext.Returns(new DefaultHttpContext() { User = user });

        var userHelperWithMissingOrgData = new UserHelper(_httpContextAccessorMock, _planTechDbContextMock, _createEstablishmentCommandMock);

        var result = await userHelperWithMissingOrgData.GetEstablishmentId();

        Assert.Equal(1, result);
    }

}