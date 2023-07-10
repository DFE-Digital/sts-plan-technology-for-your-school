using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Users.Helper
{
    public class UserHelper : IUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPlanTechDbContext _db;

        public UserHelper(IHttpContextAccessor httpContextAccessor, IPlanTechDbContext db) 
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
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
            //var establishment = claims.Where(x => x.Type.Contains("organisation")).FirstOrDefault().Value;
            //var test = establishment.Replace(@"\", "").Trim('"');
            //var establishmentparse = JsonSerializer.Deserialize<Establishment>(test);
            return 1;
        }
    }
}
