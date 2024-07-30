using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Dfe.PlanTech.Application.SignIns.Interfaces;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Infrastructure.SignIn.Services;

public class DfePublicApiService : IDfePublicApi
{
    private readonly IDfeSignInConfiguration _dfeSignInConfiguration;
    private readonly string _urlTemplate = "{0}/services/{1}/organisations/{2}/users/{3}";
    private readonly HttpClient _httpClient;


    public DfePublicApiService(IDfeSignInConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _dfeSignInConfiguration = configuration;
    }

    public async Task<UserAccessToService?> GetUserAccessToService(string userId, string organisationId)
    {
        var url = string.Format(_urlTemplate, _dfeSignInConfiguration.APIServiceProxyUrl, _dfeSignInConfiguration.ClientId, organisationId, userId);

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var userAccess = JsonSerializer.Deserialize<UserAccessToService>(responseString);

            return userAccess;
        }

        throw new UserAccessUnavailableException($"Error getting user access to service from API response code received: {response.StatusCode}");
    }

    public static string GenerateToken(string secret, string issuer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: "signin.education.gov.uk",
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials
        );

        var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenAsString;
    }
}
