using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Users.Helper;

public class UserHelper : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICreateEstablishmentCommand _createEstablishmentCommand;
    private readonly IGetUserIdQuery _getUserIdQuery;
    private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;

    public UserHelper(IHttpContextAccessor httpContextAccessor,
                        IPlanTechDbContext db,
                        ICreateEstablishmentCommand createEstablishmentCommand,
                        IGetUserIdQuery getUserIdQuery,
                        IGetEstablishmentIdQuery getEstablishmentIdQuery)
    {
        _httpContextAccessor = httpContextAccessor;
        _createEstablishmentCommand = createEstablishmentCommand;
        _getUserIdQuery = getUserIdQuery;
        _getEstablishmentIdQuery = getEstablishmentIdQuery;
    }

    public async Task<int?> GetCurrentUserId()
    {
        var dbUserId = GetDbIdFromClaim(ClaimConstants.DB_USER_ID);
        if (dbUserId != null)
            return dbUserId;

        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;

        if (userId is null)
            return null;

        return await _getUserIdQuery.GetUserId(userId);
    }

    public async Task<int> GetEstablishmentId()
    {
        var dbEstablishmentId = GetDbIdFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
        if (dbEstablishmentId != null)
            return dbEstablishmentId.Value;

        var establishmentDto = GetOrganisationData();

        var establishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(establishmentDto.Reference) ?? await SetEstablishment();

        return Convert.ToInt16(establishmentId);
    }

    public async Task<int> SetEstablishment()
    {
        var establishmentDto = GetOrganisationData();

        var establishmentId = await _createEstablishmentCommand.CreateEstablishment(establishmentDto);

        return establishmentId;
    }

    public EstablishmentDto GetOrganisationData()
    {
        var orgDetails = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains(ClaimConstants.Organisation))?.Value ??
                    throw new KeyNotFoundException($"Could not find {ClaimConstants.Organisation} claim type");

        var establishment = JsonSerializer.Deserialize<EstablishmentDto>(orgDetails);
        if (establishment == null || !establishment.IsValid)
            throw new InvalidDataException("Establishment was not expected format");

        return establishment;
    }

    private int? GetDbIdFromClaim(string type)
    {
        var id = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;

        return id != null ? int.Parse(id) : null;
    }
}
