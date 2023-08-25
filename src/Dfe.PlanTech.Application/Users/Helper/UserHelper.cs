using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Establishments.Models;
using Microsoft.AspNetCore.Http;
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
            var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();
            var userId = claims?.Find(x => x.Type.Contains("nameidentifier"))?.Value;

            if (userId is null)
                return null;

            return await _getUserIdQuery.GetUserId(userId);
        }

        public async Task<int> GetEstablishmentId()
        {
            var establishmentDto = _GetOrganisationData();

            var reference = establishmentDto.Urn ?? establishmentDto.Ukprn;

            if (reference is null)
                return 1;

            var existingEstablishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(reference);

            if (existingEstablishmentId == null)
            {
                await SetEstablishment();
                var newEstablishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(reference);
                return newEstablishmentId is null ? 1 : Convert.ToUInt16(newEstablishmentId);
            }

            return Convert.ToInt16(existingEstablishmentId);
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
    }
}
