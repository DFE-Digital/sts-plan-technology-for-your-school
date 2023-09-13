using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Dfe.PlanTech.Application.Users.Helper
{
    public class UserHelper : IUser
    {
        private const string USER_ID_IDENTIFIER = "db_user_id";
        private const string ETABLISHMENT_ID_IDENTIFIER = "db_establishment_id";

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
            var dbUserId = GetDbIdFromClaim(USER_ID_IDENTIFIER);
            if (dbUserId != null) return dbUserId;

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;

            if (userId is null)
                return null;

            var fromDb = await _getUserIdQuery.GetUserId(userId);

            if (fromDb != null) SetDbIdAsClaim(USER_ID_IDENTIFIER, fromDb.Value);

            return fromDb;
        }

        public async Task<int> GetEstablishmentId()
        {
            var dbEstablishmentId = GetDbIdFromClaim(ETABLISHMENT_ID_IDENTIFIER);
            if (dbEstablishmentId != null) return dbEstablishmentId.Value;

            var establishmentDto = _GetOrganisationData();

            var reference = (establishmentDto.Urn ?? establishmentDto.Ukprn) ?? throw new Exception("Establishment has no Urn nor Ukprn");

            var establishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(reference) ?? await SetEstablishment();

            SetDbIdAsClaim(ETABLISHMENT_ID_IDENTIFIER, establishmentId);

            var asShort = Convert.ToInt16(establishmentId);
            return asShort;
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
        private void SetDbIdAsClaim(string type, int value)
        {
            _httpContextAccessor.HttpContext.User.AddIdentity(new ClaimsIdentity(new[] { new Claim(type, value.ToString()) }));
        }
    }
}
