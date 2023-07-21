using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Establishments.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Users.Helper
{
    public class UserHelper : IUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPlanTechDbContext _db;
        private readonly ICreateEstablishmentCommand _createEstablishmentCommand;

        public UserHelper(IHttpContextAccessor httpContextAccessor, IPlanTechDbContext db, ICreateEstablishmentCommand createEstablishmentCommand)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
            _createEstablishmentCommand = createEstablishmentCommand;
        }

        public async Task<int?> GetCurrentUserId()
        {
            var claims = _httpContextAccessor.HttpContext.User.Claims.ToList();
            var userId = claims?.Find(x => x.Type.Contains("nameidentifier"))?.Value;

            if (userId is null)
                return null;

            var getUserIdQuery = new GetUserIdQuery(_db);
            return await getUserIdQuery.GetUserId(userId);
        }

        public async Task<int?> GetEstablishmentId()
        {
            var getEstablishmentIdQuery = new GetEstablishmentIdQuery(_db);

            var existingEstablishmentId = await getEstablishmentIdQuery.GetEstablishmentId(_GetOrganisationData().EstablishmentRef);
            
            if (existingEstablishmentId == null)
            { 
                await SetEstablishment();
                var newEstablishmentId =  await getEstablishmentIdQuery.GetEstablishmentId(_GetOrganisationData().EstablishmentRef);
                return newEstablishmentId;
            }

            return existingEstablishmentId;
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

            return establishment;
        }
    }
}
