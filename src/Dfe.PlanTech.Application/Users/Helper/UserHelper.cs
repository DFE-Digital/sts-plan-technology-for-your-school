using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Dfe.PlanTech.Application.Users.Helper
{
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
            if (dbUserId != null) return dbUserId;

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;

            if (userId is null)
                return null;

            return await _getUserIdQuery.GetUserId(userId);
        }

        public async Task<int> GetEstablishmentId()
        {
            var dbEstablishmentId = GetDbIdFromClaim(ClaimConstants.DB_ESTABLISHMENT_ID);
            if (dbEstablishmentId != null) return dbEstablishmentId.Value;

            var establishmentDto = _GetOrganisationData();

            var reference = (establishmentDto.Urn ?? establishmentDto.Ukprn) ?? throw new Exception("Establishment has no Urn nor Ukprn");

            var establishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(reference) ?? await SetEstablishment();

            return Convert.ToInt16(establishmentId);;
        }

        public async Task<int> SetEstablishment()
        {
            var establishmentDto = _GetOrganisationData();

            var establishmentId = await _createEstablishmentCommand.CreateEstablishment(establishmentDto);

            return establishmentId;
        }

        private EstablishmentDto _GetOrganisationData()
        {

            var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();
            var orgDetails = claims?.Find(x => x.Type.Contains("organisation"))?.Value;

            orgDetails ??= "{}";
            var establishment = JsonSerializer.Deserialize<EstablishmentDto>(orgDetails);

            establishment ??= new EstablishmentDto();

            return establishment;
        }

        private int? GetDbIdFromClaim(string type)
        {
            var id = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;

            return id != null ? int.Parse(id) : null;
        }
    }
}
