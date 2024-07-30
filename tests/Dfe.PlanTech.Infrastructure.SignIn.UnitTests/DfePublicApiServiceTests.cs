using System.Diagnostics;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Dfe.PlanTech.Infrastructure.SignIn.Services;
using NSubstitute;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

public class DfePublicApiServiceTests : IAsyncLifetime
{
    private IDfeSignInConfiguration _configuration = null!;
    private WireMockServer _wireMockServer = null!;
    private DfePublicApiService _dfePublicApiService = null!;

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();

        _configuration = Substitute.For<IDfeSignInConfiguration>();
        _configuration.APIServiceProxyUrl.Returns(_wireMockServer.Urls[0]);
        _configuration.ClientId.Returns("PLANTECH");
        _configuration.ApiSecret.Returns("This is an api secret that is very secret");
        HttpClient httpClient = new HttpClient();

        _dfePublicApiService = new DfePublicApiService(_configuration, httpClient);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer.Stop();
        return Task.CompletedTask;
    }

    [Fact]
    public void Token_Returned_When_Generate_Token_Is_Called()
    {

        var token = DfePublicApiService.GenerateToken(_configuration.ApiSecret, "test");

        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task User_Access_Object_Contains_The_Correct_Role_Code_When_Calling_GetUserAccessToService()
    {
        _wireMockServer
            .Given(
                Request.Create().WithPath("/services/PLANTECH/organisations/44444444-4444-4444-9B60-4A04BD29AE0E/users/33333333-3333-3333-93DD-291088CD3788").UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("{\n    \"userId\": \"33333333-3333-3333-93DD-291088CD3788\",\n    \"userLegacyNumericId\": \"72014\",\n    \"userLegacyTextId\": \"39jr9n4\",\n    \"serviceId\": \"909C467C-6B73-45F0-9548-8062152FA7D3\",\n    \"organisationId\": \"44444444-4444-4444-9B60-4A04BD29AE0E\",\n    \"organisationLegacyId\": \"4008569\",\n    \"roles\": [\n        {\n            \"id\": \"5D80B58F-C726-4669-8CFE-A8776BD11A04\",\n            \"name\": \"Plan Tech For School - For Establishment Use Only\",\n            \"code\": \"plan_tech_for_school_estalishment_only\",\n            \"numericId\": \"22149\",\n            \"status\": {\n                \"id\": 1\n            }\n        }\n    ],\n    \"identifiers\": []\n}")
            );

        var userAccessToService = await _dfePublicApiService.GetUserAccessToService("33333333-3333-3333-93DD-291088CD3788", "44444444-4444-4444-9B60-4A04BD29AE0E");

        Debug.Assert(userAccessToService != null, nameof(userAccessToService) + " != null");
        Assert.Equal("plan_tech_for_school_estalishment_only", userAccessToService.Roles[0].Code);
        Assert.Equal(Guid.Parse("33333333-3333-3333-93DD-291088CD3788"), userAccessToService.UserId);
        Assert.Equal(Guid.Parse("44444444-4444-4444-9B60-4A04BD29AE0E"), userAccessToService.OrganisationId);
        Assert.Equal(Guid.Parse("909C467C-6B73-45F0-9548-8062152FA7D3"), userAccessToService.ServiceId);
    }


    [Fact]
    public async Task DfeApiService_Throw_Exception_If_Call_To_Dfe_UserAccessApi_Unsuccessful()
    {
        _wireMockServer
            .Given(
                Request.Create().WithPath("/services/PLANTECH/organisations/44444444-4444-4444-9B60-4A04BD29AE0E/users/33333333-3333-3333-93DD-291088CD3788").UsingGet()
            )
            .RespondWith(
                Response.Create()
                    .WithStatusCode(418)
            );

        await Assert.ThrowsAnyAsync<UserAccessUnavailableException>(() => _dfePublicApiService.GetUserAccessToService("33333333-3333-3333-93DD-291088CD3788", "44444444-4444-4444-9B60-4A04BD29AE0E"));

    }
}
