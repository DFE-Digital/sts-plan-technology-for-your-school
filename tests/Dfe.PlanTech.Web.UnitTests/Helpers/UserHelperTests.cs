using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class UserHelperTests
{
    private const string FIRST_USER_REF = "1";
    private const string SECOND_USER_REF = "OtherReference";

    private const string FIRST_ESTABLISHMENT_REF = "131";

    private readonly UserHelper _userHelper;
    private IHttpContextAccessor _httpContextAccessorMock;
    private IPlanTechDbContext _planTechDbContextMock;
    private ICreateEstablishmentCommand _createEstablishmentCommandMock;
    private IGetUserIdQuery _getUserIdQueryMock;
    private IGetEstablishmentIdQuery _getEstablishmentIdQueryMock;

    private List<User> _users = new(){
        new User(){
            DfeSignInRef = FIRST_USER_REF,
            Id = 1
        },
        new User(){
            DfeSignInRef = SECOND_USER_REF,
            Id = 2
        }
    };

    private List<Establishment> _establishments = new(){
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
        _httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        _planTechDbContextMock = Substitute.For<IPlanTechDbContext>();
        _createEstablishmentCommandMock = Substitute.For<ICreateEstablishmentCommand>();
        _getUserIdQueryMock = Substitute.For<IGetUserIdQuery>();
        _getEstablishmentIdQueryMock = Substitute.For<IGetEstablishmentIdQuery>(); 

        MockGetEstablishmentIdQuery();
        MockGetUserIdQuery();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"ukprn\" : \"131\",\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
            }, "mock"));

        _httpContextAccessorMock.HttpContext.Returns(new DefaultHttpContext() { User = user });

        _userHelper = new UserHelper(_httpContextAccessorMock, _planTechDbContextMock, _createEstablishmentCommandMock, _getUserIdQueryMock, _getEstablishmentIdQueryMock);
    }

    private void MockGetEstablishmentIdQuery()
    {
        _getEstablishmentIdQueryMock = Substitute.For<IGetEstablishmentIdQuery>();
        _getEstablishmentIdQueryMock.GetEstablishmentId(Arg.Any<string>())
                                    .Returns((callInfo) =>
                                    {
                                        var establishmentRef = callInfo.ArgAt<string>(0);

                                        return _establishments.Where(establishment => establishment.EstablishmentRef == establishmentRef)
                                                    .Select(establishment => establishment.Id)
                                                    .FirstOrDefault();
                                    });
    }

    private void MockGetUserIdQuery()
    {
        _getUserIdQueryMock = Substitute.For<IGetUserIdQuery>();
        _getUserIdQueryMock.GetUserId(Arg.Any<string>()).Returns((callInfo) =>
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

        await _createEstablishmentCommandMock.Received(0).CreateEstablishment(Arg.Any<EstablishmentDto>());

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
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

        var userHelperWithMissingOrgData = new UserHelper(_httpContextAccessorMock, _planTechDbContextMock, _createEstablishmentCommandMock, _getUserIdQueryMock, _getEstablishmentIdQueryMock);

        var result = await userHelperWithMissingOrgData.GetEstablishmentId();

        Assert.Equal(1, result);
    }

}