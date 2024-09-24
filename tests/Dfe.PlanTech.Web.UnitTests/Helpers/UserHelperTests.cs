using System.Security.Claims;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class UserHelperTests
{
    private const string FIRST_USER_REF = "1";
    private const string SECOND_USER_REF = "OtherReference";

    private const string FIRST_ESTABLISHMENT_REF = "131";

    private readonly UserHelper _userHelper;
    private readonly IHttpContextAccessor _httpContextAccessorSubstitute;
    private readonly IPlanTechDbContext _planTechDbContextSubstitute;
    private readonly ICreateEstablishmentCommand _createEstablishmentCommandSubstitute;
    private IGetUserIdQuery _getUserIdQuerySubstitute;
    private IGetEstablishmentIdQuery _getEstablishmentIdQuerySubstitute;

    private readonly List<User> _users = new(){
        new User(){
            DfeSignInRef = FIRST_USER_REF,
            Id = 1
        },
        new User(){
            DfeSignInRef = SECOND_USER_REF,
            Id = 2
        }
    };

    private readonly List<Establishment> _establishments = new(){
       new Establishment(){
        EstablishmentRef = FIRST_ESTABLISHMENT_REF,
        Id = 1
       },
       new Establishment(){
        EstablishmentRef = "Other reference",
        Id = 2
       }
    };


    public UserHelperTests()
    {
        _httpContextAccessorSubstitute = Substitute.For<IHttpContextAccessor>();
        _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();
        _createEstablishmentCommandSubstitute = Substitute.For<ICreateEstablishmentCommand>();
        _getUserIdQuerySubstitute = Substitute.For<IGetUserIdQuery>();
        _getEstablishmentIdQuerySubstitute = Substitute.For<IGetEstablishmentIdQuery>();

        SubstituteGetEstablishmentIdQuery();
        SubstituteGetUserIdQuery();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"ukprn\" : \"131\",\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
            }, ""));

        _httpContextAccessorSubstitute.HttpContext.Returns(new DefaultHttpContext() { User = user });

        _userHelper = new UserHelper(_httpContextAccessorSubstitute, _planTechDbContextSubstitute, _createEstablishmentCommandSubstitute, _getUserIdQuerySubstitute, _getEstablishmentIdQuerySubstitute);
    }

    private void SubstituteGetEstablishmentIdQuery()
    {
        _getEstablishmentIdQuerySubstitute = Substitute.For<IGetEstablishmentIdQuery>();
        _getEstablishmentIdQuerySubstitute.GetEstablishmentId(Arg.Any<string>())
                                    .Returns((callInfo) =>
                                    {
                                        var establishmentRef = callInfo.ArgAt<string>(0);

                                        return _establishments.Where(establishment => establishment.EstablishmentRef == establishmentRef)
                                                    .Select(establishment => establishment.Id)
                                                    .FirstOrDefault();
                                    });
    }

    private void SubstituteGetUserIdQuery()
    {
        _getUserIdQuerySubstitute = Substitute.For<IGetUserIdQuery>();
        _getUserIdQuerySubstitute.GetUserId(Arg.Any<string>()).Returns((callInfo) =>
        {
            var userRef = callInfo.ArgAt<string>(0);

            return _users.Where(user => user.DfeSignInRef == userRef)
                        .Select(user => user.Id)
                        .FirstOrDefault();
        });
    }

    [Fact]
    public async Task GetCurrentUserId_Returns_Correct_Id_When_UserExists_InDatabase()
    {
        var result = await _userHelper.GetCurrentUserId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetEstablishmentId_Returns_Correct_Id_When_Establishment_Exists_In_DB()
    {
        var result = await _userHelper.GetEstablishmentId();

        await _createEstablishmentCommandSubstitute.Received(0).CreateEstablishment(Arg.Any<EstablishmentDto>());

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }


    [Fact]
    public async Task GetEstablishmentId_Throws_Exception_When_Reference_Is_Not_Present()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
        }, ""));

        _httpContextAccessorSubstitute.HttpContext.Returns(new DefaultHttpContext() { User = user });

        var userHelperWithMissingOrgData = new UserHelper(_httpContextAccessorSubstitute, _planTechDbContextSubstitute, _createEstablishmentCommandSubstitute, _getUserIdQuerySubstitute, _getEstablishmentIdQuerySubstitute);

        await Assert.ThrowsAnyAsync<Exception>(() => userHelperWithMissingOrgData.GetEstablishmentId());
    }

}
