using System.Security.Claims;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class UserHelperTests
{
    private readonly UserHelper _userHelper;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;

    public UserHelperTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _planTechDbContextMock = new Mock<IPlanTechDbContext>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "mock"));

        _httpContextAccessorMock.Setup(m => m.HttpContext).Returns(new DefaultHttpContext() { User = user });

        _userHelper = new UserHelper(_httpContextAccessorMock.Object, _planTechDbContextMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserId_Returns_Correct_Id_When_UserExists_InDatabase()
    {
        _planTechDbContextMock.Setup(m => m.GetUserBy(userModel => userModel.DfeSignInRef == "1")).ReturnsAsync(new User() { Id = 1 });

        var result = await _userHelper.GetCurrentUserId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }

    [Fact]
    public void GetEstablishmentId_Returns_Correct_Id()
    {
        var result = _userHelper.GetEstablishmentId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }
}