using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Application.Providers;

public class DsiOrganisationProvider(DfeSignInConfiguration dsiConfig, HttpClient httpClient)
    : IDsiOrganisationProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _clientId = dsiConfig.ClientId;
    private readonly string _apiSecret = dsiConfig.ApiSecret;

    public async Task<EstablishmentModel?> GetOrganisationForUserAsync(
        string userDsiReference,
        string urn
    )
    {
        var userOrganisations = await FetchOrganisationDataAsync(userDsiReference);
        return userOrganisations.SingleOrDefault(uo => string.Equals(uo.Urn, urn));
    }

    private async Task<IEnumerable<EstablishmentModel>> FetchOrganisationDataAsync(
        string userDsiReference
    )
    {
        var token = GenerateJwt();

        var organisationsDataEndpoint = $"users/{userDsiReference}/organisations";

        using var request = new HttpRequestMessage(HttpMethod.Get, organisationsDataEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<IEnumerable<EstablishmentModel>>(json) ?? [];
    }

    private string GenerateJwt()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _clientId,
            Audience = "signin.education.gov.uk",
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = credentials,
            // jwt.sign's default behaviour also stamps an "iat" claim - JwtSecurityTokenHandler
            // does this automatically, so no extra claim needed here.
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}
